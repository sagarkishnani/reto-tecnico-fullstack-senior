using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Autenticacion.UseCases;
using Pedidos.Application.Pedidos.UseCases;

namespace Pedidos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IObtenerPedidosUseCase, ObtenerPedidosUseCase>();
        services.AddScoped<IObtenerPedidoPorIdUseCase, ObtenerPedidoPorIdUseCase>();
        services.AddScoped<ICrearPedidoUseCase, CrearPedidoUseCase>();
        services.AddScoped<IActualizarPedidoUseCase, ActualizarPedidoUseCase>();
        services.AddScoped<IEliminarPedidoUseCase, EliminarPedidoUseCase>();
        services.AddScoped<IIniciarSesionUseCase, IniciarSesionUseCase>();

        return services;
    }
}
