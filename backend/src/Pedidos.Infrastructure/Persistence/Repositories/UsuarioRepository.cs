using Microsoft.EntityFrameworkCore;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Repositories;

namespace Pedidos.Infrastructure.Persistence.Repositories;

internal sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly PedidosDbContext _context;

    public UsuarioRepository(PedidosDbContext context)
    {
        _context = context;
    }

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizado = email.Trim().ToLowerInvariant();

        return _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Email == normalizado, cancellationToken);
    }
}
