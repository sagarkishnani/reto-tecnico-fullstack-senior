using Pedidos.Application.Pedidos.Dtos;
using Pedidos.Application.Pedidos.Mapping;
using Pedidos.Domain.Repositories;

namespace Pedidos.Application.Pedidos.UseCases;

internal sealed class ObtenerPedidosUseCase : IObtenerPedidosUseCase
{
    private readonly IPedidoRepository _pedidoRepository;

    public ObtenerPedidosUseCase(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<IReadOnlyList<PedidoDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var pedidos = await _pedidoRepository.ObtenerTodosAsync(cancellationToken);

        return pedidos.Select(pedido => pedido.ADto()).ToList();
    }
}
