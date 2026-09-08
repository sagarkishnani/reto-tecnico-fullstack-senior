using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pedidos.Application.Abstractions.Security;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;

namespace Pedidos.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly PedidosDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        PedidosDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task InicializarAsync(CancellationToken cancellationToken = default)
    {
        var pendientes = await _context.Database.GetPendingMigrationsAsync(cancellationToken);

        if (pendientes.Any())
        {
            _logger.LogInformation("Aplicando {Cantidad} migracion(es) pendiente(s).", pendientes.Count());
            await _context.Database.MigrateAsync(cancellationToken);
        }

        await SembrarUsuariosAsync(cancellationToken);
        await SembrarPedidosAsync(cancellationToken);
    }

    private async Task SembrarUsuariosAsync(CancellationToken cancellationToken)
    {
        if (await _context.Usuarios.AnyAsync(cancellationToken))
        {
            return;
        }

        _context.Usuarios.AddRange(
            Usuario.Crear("user@email.com", _passwordHasher.Hashear("123456"), "Usuario Demo", RolUsuario.User),
            Usuario.Crear("admin@email.com", _passwordHasher.Hashear("Admin123*"), "Administrador", RolUsuario.Admin));

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Usuarios semilla creados.");
    }

    private async Task SembrarPedidosAsync(CancellationToken cancellationToken)
    {
        if (await _context.Pedidos.AnyAsync(cancellationToken))
        {
            return;
        }

        var hoy = DateTime.UtcNow.Date;

        var pedidos = new[]
        {
            Pedido.Crear("PED-001", "Juan Perez", hoy.AddDays(-6), 250.75m),
            Pedido.Crear("PED-002", "Ana Diaz", hoy.AddDays(-4), 1299.00m),
            Pedido.Crear("PED-003", "Carlos Mendoza", hoy.AddDays(-2), 87.50m),
            Pedido.Crear("PED-004", "Lucia Ramos", hoy, 4310.20m)
        };

        pedidos[1].CambiarEstado(EstadoPedido.EnProceso);
        pedidos[2].CambiarEstado(EstadoPedido.Entregado);

        _context.Pedidos.AddRange(pedidos);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Pedidos semilla creados.");
    }
}
