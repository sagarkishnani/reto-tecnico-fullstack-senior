using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Email)
            .HasMaxLength(Usuario.LongitudMaximaEmail)
            .IsRequired();

        builder.Property(usuario => usuario.PasswordHash)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(usuario => usuario.Nombre)
            .HasMaxLength(Usuario.LongitudMaximaNombre)
            .IsRequired();

        builder.Property(usuario => usuario.Rol)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(usuario => usuario.FechaCreacion)
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique()
            .HasFilter("\"Eliminado\" = false")
            .HasDatabaseName("IX_Usuarios_Email_Activos");

        builder.HasQueryFilter(usuario => !usuario.Eliminado);
    }
}
