using Pedidos.Application.Abstractions.Persistence;
using Pedidos.Domain.Exceptions;
using Pedidos.Domain.Repositories;

namespace Pedidos.Application.Pedidos.UseCases;

internal sealed class EliminarPedidoUseCase : IEliminarPedidoUseCase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarPedidoUseCase(IPedidoRepository pedidoRepository, IUnitOfWork unitOfWork)
    {
        _pedidoRepository = pedidoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException("Pedido", id);

        pedido.Eliminar();

        _pedidoRepository.Actualizar(pedido);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);
    }
}
