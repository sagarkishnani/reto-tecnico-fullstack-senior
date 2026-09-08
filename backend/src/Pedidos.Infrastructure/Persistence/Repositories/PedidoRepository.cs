using Microsoft.EntityFrameworkCore;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Repositories;
using Pedidos.Infrastructure.Resiliencia;

namespace Pedidos.Infrastructure.Persistence.Repositories;

internal sealed class PedidoRepository : IPedidoRepository
{
    private readonly PedidosDbContext _context;
    private readonly IEjecutorDeBaseDeDatos _ejecutor;

    public PedidoRepository(PedidosDbContext context, IEjecutorDeBaseDeDatos ejecutor)
    {
        _context = context;
        _ejecutor = ejecutor;
    }

    public Task<IReadOnlyList<Pedido>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        _ejecutor.LeerAsync<IReadOnlyList<Pedido>>(
            async token => await _context.Pedidos
                .AsNoTracking()
                .OrderByDescending(pedido => pedido.Fecha)
                .ThenByDescending(pedido => pedido.Id)
                .ToListAsync(token),
            cancellationToken);

    public Task<Pedido?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _ejecutor.LeerAsync(
            token => _context.Pedidos.FirstOrDefaultAsync(pedido => pedido.Id == id, token),
            cancellationToken);

    public Task<bool> ExisteNumeroPedidoAsync(
        string numeroPedido,
        int? idExcluido = null,
        CancellationToken cancellationToken = default)
    {
        var normalizado = numeroPedido.Trim().ToUpperInvariant();

        return _ejecutor.LeerAsync(
            token => _context.Pedidos.AnyAsync(
                pedido => pedido.NumeroPedido == normalizado && (idExcluido == null || pedido.Id != idExcluido),
                token),
            cancellationToken);
    }

    public async Task AgregarAsync(Pedido pedido, CancellationToken cancellationToken = default) =>
        await _context.Pedidos.AddAsync(pedido, cancellationToken);

    public void Actualizar(Pedido pedido) => _context.Pedidos.Update(pedido);
}
