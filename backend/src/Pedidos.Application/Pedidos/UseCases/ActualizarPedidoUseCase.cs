using Pedidos.Application.Abstractions.Persistence;
using Pedidos.Application.Pedidos.Dtos;
using Pedidos.Application.Pedidos.Mapping;
using Pedidos.Domain.Exceptions;
using Pedidos.Domain.Repositories;

namespace Pedidos.Application.Pedidos.UseCases;

internal sealed class ActualizarPedidoUseCase : IActualizarPedidoUseCase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarPedidoUseCase(IPedidoRepository pedidoRepository, IUnitOfWork unitOfWork)
    {
        _pedidoRepository = pedidoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PedidoDto> EjecutarAsync(
        int id,
        ActualizarPedidoRequest request,
        CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException("Pedido", id);

        var estado = PedidoMapper.AEstadoPedido(request.Estado);

        pedido.Actualizar(request.NumeroPedido, request.Cliente, request.Fecha, request.Total, estado);

        if (await _pedidoRepository.ExisteNumeroPedidoAsync(pedido.NumeroPedido, id, cancellationToken))
        {
            throw new NumeroPedidoDuplicadoException(pedido.NumeroPedido);
        }

        _pedidoRepository.Actualizar(pedido);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return pedido.ADto();
    }
}
