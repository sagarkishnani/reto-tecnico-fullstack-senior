using Pedidos.Application.Pedidos.Dtos;
using Pedidos.Application.Pedidos.Mapping;
using Pedidos.Domain.Exceptions;
using Pedidos.Domain.Repositories;

namespace Pedidos.Application.Pedidos.UseCases;

internal sealed class ObtenerPedidoPorIdUseCase : IObtenerPedidoPorIdUseCase
{
    private readonly IPedidoRepository _pedidoRepository;

    public ObtenerPedidoPorIdUseCase(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<PedidoDto> EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException("Pedido", id);

        return pedido.ADto();
    }
}
