using Pedidos.Domain.Entities;

namespace Pedidos.Domain.Repositories;

public interface IPedidoRepository
{
    Task<IReadOnlyList<Pedido>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    Task<Pedido?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExisteNumeroPedidoAsync(
        string numeroPedido,
        int? idExcluido = null,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(Pedido pedido, CancellationToken cancellationToken = default);

    void Actualizar(Pedido pedido);
}
