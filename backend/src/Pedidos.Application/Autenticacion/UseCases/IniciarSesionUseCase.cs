using Pedidos.Application.Abstractions.Security;
using Pedidos.Application.Autenticacion.Dtos;
using Pedidos.Domain.Exceptions;
using Pedidos.Domain.Repositories;

namespace Pedidos.Application.Autenticacion.UseCases;

internal sealed class IniciarSesionUseCase : IIniciarSesionUseCase
{
    private const string HashDeReferencia = "$2a$12$eImiTXuWVxfM37uY4JANjQ.iC1DlpQoDMPbFI4uEIRZQ8Gs8Zi3Vy";

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IGeneradorDeToken _generadorDeToken;

    public IniciarSesionUseCase(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IGeneradorDeToken generadorDeToken)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _generadorDeToken = generadorDeToken;
    }

    public async Task<LoginResponse> EjecutarAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new CredencialesInvalidasException();
        }

        var usuario = await _usuarioRepository.ObtenerPorEmailAsync(request.Email, cancellationToken);

        if (usuario is null)
        {
            _passwordHasher.Verificar(request.Password, HashDeReferencia);
            throw new CredencialesInvalidasException();
        }

        if (!_passwordHasher.Verificar(request.Password, usuario.PasswordHash))
        {
            throw new CredencialesInvalidasException();
        }

        var token = _generadorDeToken.Generar(usuario);

        return new LoginResponse(token.Token, token.ExpiraEnSegundos);
    }
}
