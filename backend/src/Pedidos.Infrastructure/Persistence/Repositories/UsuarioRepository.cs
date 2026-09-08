using Microsoft.EntityFrameworkCore;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Repositories;
using Pedidos.Infrastructure.Resiliencia;

namespace Pedidos.Infrastructure.Persistence.Repositories;

internal sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly PedidosDbContext _context;
    private readonly IEjecutorDeBaseDeDatos _ejecutor;

    public UsuarioRepository(PedidosDbContext context, IEjecutorDeBaseDeDatos ejecutor)
    {
        _context = context;
        _ejecutor = ejecutor;
    }

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _ejecutor.LeerAsync(
            token => _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(usuario => usuario.Id == id, token),
            cancellationToken);

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizado = email.Trim().ToLowerInvariant();

        return _ejecutor.LeerAsync(
            token => _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(usuario => usuario.Email == normalizado, token),
            cancellationToken);
    }
}
