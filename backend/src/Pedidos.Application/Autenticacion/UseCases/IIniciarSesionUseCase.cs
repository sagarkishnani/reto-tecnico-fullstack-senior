using Pedidos.Application.Autenticacion.Dtos;

namespace Pedidos.Application.Autenticacion.UseCases;

public interface IIniciarSesionUseCase
{
    Task<LoginResponse> EjecutarAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
