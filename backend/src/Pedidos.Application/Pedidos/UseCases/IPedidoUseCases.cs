using Pedidos.Application.Pedidos.Dtos;

namespace Pedidos.Application.Pedidos.UseCases;

public interface IObtenerPedidosUseCase
{
    Task<IReadOnlyList<PedidoDto>> EjecutarAsync(CancellationToken cancellationToken = default);
}

public interface IObtenerPedidoPorIdUseCase
{
    Task<PedidoDto> EjecutarAsync(int id, CancellationToken cancellationToken = default);
}

public interface ICrearPedidoUseCase
{
    Task<PedidoDto> EjecutarAsync(CrearPedidoRequest request, CancellationToken cancellationToken = default);
}

public interface IActualizarPedidoUseCase
{
    Task<PedidoDto> EjecutarAsync(int id, ActualizarPedidoRequest request, CancellationToken cancellationToken = default);
}

public interface IEliminarPedidoUseCase
{
    Task EjecutarAsync(int id, CancellationToken cancellationToken = default);
}
