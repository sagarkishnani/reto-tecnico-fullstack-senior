using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Configurations;

internal sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(pedido => pedido.Id);

        builder.Property(pedido => pedido.NumeroPedido)
            .HasMaxLength(Pedido.LongitudMaximaNumeroPedido)
            .IsRequired();

        builder.Property(pedido => pedido.Cliente)
            .HasMaxLength(Pedido.LongitudMaximaCliente)
            .IsRequired();

        builder.Property(pedido => pedido.Fecha)
            .IsRequired();

        builder.Property(pedido => pedido.Total)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(pedido => pedido.Estado)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pedido => pedido.FechaCreacion)
            .IsRequired();

        builder.HasIndex(pedido => pedido.NumeroPedido)
            .IsUnique()
            .HasFilter("\"Eliminado\" = false")
            .HasDatabaseName(PedidosDbContext.IndiceNumeroPedidoUnico);

        builder.HasQueryFilter(pedido => !pedido.Eliminado);
    }
}
