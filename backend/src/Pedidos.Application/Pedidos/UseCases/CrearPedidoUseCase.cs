using Pedidos.Application.Abstractions.Persistence;
using Pedidos.Application.Pedidos.Dtos;
using Pedidos.Application.Pedidos.Mapping;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;
using Pedidos.Domain.Repositories;

namespace Pedidos.Application.Pedidos.UseCases;

internal sealed class CrearPedidoUseCase : ICrearPedidoUseCase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearPedidoUseCase(IPedidoRepository pedidoRepository, IUnitOfWork unitOfWork)
    {
        _pedidoRepository = pedidoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PedidoDto> EjecutarAsync(
        CrearPedidoRequest request,
        CancellationToken cancellationToken = default)
    {
        var pedido = Pedido.Crear(request.NumeroPedido, request.Cliente, request.Fecha, request.Total);

        if (await _pedidoRepository.ExisteNumeroPedidoAsync(pedido.NumeroPedido, null, cancellationToken))
        {
            throw new NumeroPedidoDuplicadoException(pedido.NumeroPedido);
        }

        await _pedidoRepository.AgregarAsync(pedido, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return pedido.ADto();
    }
}
