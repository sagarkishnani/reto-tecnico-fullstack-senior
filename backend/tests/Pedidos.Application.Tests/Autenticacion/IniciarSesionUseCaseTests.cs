using Pedidos.Application.Autenticacion.Dtos;
using Pedidos.Application.Autenticacion.UseCases;
using Pedidos.Application.Tests.Dobles;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Application.Tests.Autenticacion;

public class IniciarSesionUseCaseTests
{
    private readonly UsuarioRepositoryEnMemoria _usuarios = new();
    private readonly PasswordHasherFalso _hasher = new();
    private readonly GeneradorDeTokenFalso _generador = new();
    private readonly IniciarSesionUseCase _useCase;

    public IniciarSesionUseCaseTests()
    {
        _usuarios.Sembrar(Usuario.Crear("user@email.com", "hash::123456", "Usuario Demo", RolUsuario.User));
        _useCase = new IniciarSesionUseCase(_usuarios, _hasher, _generador);
    }

    [Fact]
    public async Task EjecutarAsync_ConCredencialesCorrectas_DevuelveTokenYExpiracion()
    {
        var resultado = await _useCase.EjecutarAsync(new LoginRequest("user@email.com", "123456"));

        Assert.Equal("token-de-user@email.com", resultado.Token);
        Assert.Equal(3600, resultado.ExpiresIn);
    }

    [Fact]
    public async Task EjecutarAsync_NormalizaElEmailAntesDeBuscar()
    {
        var resultado = await _useCase.EjecutarAsync(new LoginRequest("  USER@EMAIL.COM  ", "123456"));

        Assert.Equal("token-de-user@email.com", resultado.Token);
    }

    [Fact]
    public async Task EjecutarAsync_ConPasswordIncorrecta_LanzaCredencialesInvalidas()
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _useCase.EjecutarAsync(new LoginRequest("user@email.com", "incorrecta")));

        Assert.Null(_generador.UsuarioRecibido);
    }

    [Fact]
    public async Task EjecutarAsync_ConEmailInexistente_LanzaCredencialesInvalidas()
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _useCase.EjecutarAsync(new LoginRequest("nadie@email.com", "123456")));
    }

    [Fact]
    public async Task EjecutarAsync_ConEmailInexistente_IgualVerificaUnHashParaNoRevelarQuienExiste()
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _useCase.EjecutarAsync(new LoginRequest("nadie@email.com", "123456")));

        Assert.Equal(1, _hasher.VecesQueSeVerifico);
    }

    [Fact]
    public async Task EjecutarAsync_ConEmailInexistenteYConPasswordIncorrecta_DevuelveElMismoMensaje()
    {
        var porEmail = await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _useCase.EjecutarAsync(new LoginRequest("nadie@email.com", "123456")));

        var porPassword = await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _useCase.EjecutarAsync(new LoginRequest("user@email.com", "incorrecta")));

        Assert.Equal(porEmail.Message, porPassword.Message);
    }

    [Theory]
    [InlineData("", "123456")]
    [InlineData("user@email.com", "")]
    [InlineData("   ", "   ")]
    public async Task EjecutarAsync_ConCamposVacios_LanzaCredencialesInvalidas(string email, string password)
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _useCase.EjecutarAsync(new LoginRequest(email, password)));
    }

    [Fact]
    public async Task EjecutarAsync_ConCredencialesCorrectas_PasaElUsuarioAlGeneradorDeToken()
    {
        await _useCase.EjecutarAsync(new LoginRequest("user@email.com", "123456"));

        Assert.NotNull(_generador.UsuarioRecibido);
        Assert.Equal("user@email.com", _generador.UsuarioRecibido.Email);
        Assert.Equal(RolUsuario.User, _generador.UsuarioRecibido.Rol);
    }
}
