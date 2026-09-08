using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Abstractions.Persistence;
using Pedidos.Application.Abstractions.Security;
using Pedidos.Domain.Repositories;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Persistence.Repositories;
using Pedidos.Infrastructure.Security;

namespace Pedidos.Infrastructure;

public static class DependencyInjection
{
    private const string NombreDeLaConexion = "Postgres";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cadenaDeConexion = configuration.GetConnectionString(NombreDeLaConexion);

        if (string.IsNullOrWhiteSpace(cadenaDeConexion))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{NombreDeLaConexion}'. " +
                "Defínela en appsettings.Development.json o en la variable de entorno ConnectionStrings__Postgres.");
        }

        services.AddDbContext<PedidosDbContext>(opciones =>
            opciones.UseNpgsql(cadenaDeConexion, npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                npgsql.MigrationsAssembly(typeof(PedidosDbContext).Assembly.GetName().Name);
            }));

        services.AddScoped<IUnitOfWork>(proveedor => proveedor.GetRequiredService<PedidosDbContext>());
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
