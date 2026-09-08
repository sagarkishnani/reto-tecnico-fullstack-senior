using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pedidos.Application.Abstractions.Persistence;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Infrastructure.Persistence;

public sealed class PedidosDbContext : DbContext, IUnitOfWork
{
    internal const string IndiceNumeroPedidoUnico = "IX_Pedidos_NumeroPedido_Activos";

    private const string CodigoViolacionDeUnicidad = "23505";

    public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options)
    {
    }

    public DbSet<Pedido> Pedidos => Set<Pedido>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException excepcion) when (EsNumeroPedidoDuplicado(excepcion))
        {
            var numeroPedido = excepcion.Entries
                .Select(entrada => entrada.Entity)
                .OfType<Pedido>()
                .Select(pedido => pedido.NumeroPedido)
                .FirstOrDefault() ?? string.Empty;

            throw new NumeroPedidoDuplicadoException(numeroPedido);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    private static bool EsNumeroPedidoDuplicado(DbUpdateException excepcion) =>
        excepcion.InnerException is PostgresException postgres
        && postgres.SqlState == CodigoViolacionDeUnicidad
        && postgres.ConstraintName == IndiceNumeroPedidoUnico;
}
