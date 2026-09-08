using Microsoft.EntityFrameworkCore;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Repositories;

namespace Pedidos.Infrastructure.Persistence.Repositories;

internal sealed class PedidoRepository : IPedidoRepository
{
    private readonly PedidosDbContext _context;

    public PedidoRepository(PedidosDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Pedido>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        await _context.Pedidos
            .AsNoTracking()
            .OrderByDescending(pedido => pedido.Fecha)
            .ThenByDescending(pedido => pedido.Id)
            .ToListAsync(cancellationToken);

    public Task<Pedido?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Pedidos.FirstOrDefaultAsync(pedido => pedido.Id == id, cancellationToken);

    public Task<bool> ExisteNumeroPedidoAsync(
        string numeroPedido,
        int? idExcluido = null,
        CancellationToken cancellationToken = default)
    {
        var normalizado = numeroPedido.Trim().ToUpperInvariant();

        return _context.Pedidos.AnyAsync(
            pedido => pedido.NumeroPedido == normalizado && (idExcluido == null || pedido.Id != idExcluido),
            cancellationToken);
    }

    public async Task AgregarAsync(Pedido pedido, CancellationToken cancellationToken = default) =>
        await _context.Pedidos.AddAsync(pedido, cancellationToken);

    public void Actualizar(Pedido pedido) => _context.Pedidos.Update(pedido);
}
