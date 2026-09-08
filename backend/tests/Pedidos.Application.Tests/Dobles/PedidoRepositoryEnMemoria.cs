using Pedidos.Domain.Entities;
using Pedidos.Domain.Repositories;

namespace Pedidos.Application.Tests.Dobles;

internal sealed class PedidoRepositoryEnMemoria : IPedidoRepository
{
    private readonly List<Pedido> _pedidos = [];
    private int _siguienteId = 1;

    public int VecesQueSeLlamoAgregar { get; private set; }

    public int VecesQueSeLlamoActualizar { get; private set; }

    public Pedido Sembrar(Pedido pedido)
    {
        AsignarId(pedido, _siguienteId++);
        _pedidos.Add(pedido);
        return pedido;
    }

    public IReadOnlyList<Pedido> Contenido => _pedidos;

    public Task<IReadOnlyList<Pedido>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Pedido>>(_pedidos.Where(pedido => !pedido.Eliminado).ToList());

    public Task<Pedido?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_pedidos.FirstOrDefault(pedido => pedido.Id == id && !pedido.Eliminado));

    public Task<bool> ExisteNumeroPedidoAsync(
        string numeroPedido,
        int? idExcluido = null,
        CancellationToken cancellationToken = default)
    {
        var normalizado = numeroPedido.Trim().ToUpperInvariant();

        return Task.FromResult(_pedidos.Any(pedido =>
            !pedido.Eliminado
            && pedido.NumeroPedido == normalizado
            && (idExcluido is null || pedido.Id != idExcluido)));
    }

    public Task AgregarAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        VecesQueSeLlamoAgregar++;
        AsignarId(pedido, _siguienteId++);
        _pedidos.Add(pedido);
        return Task.CompletedTask;
    }

    public void Actualizar(Pedido pedido) => VecesQueSeLlamoActualizar++;

    private static void AsignarId(Pedido pedido, int id) =>
        typeof(Pedido)
            .GetProperty(nameof(Pedido.Id))!
            .SetValue(pedido, id);
}
