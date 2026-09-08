using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Abstractions.Persistence;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence;

public sealed class PedidosDbContext : DbContext, IUnitOfWork
{
    public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options)
    {
    }

    public DbSet<Pedido> Pedidos => Set<Pedido>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
