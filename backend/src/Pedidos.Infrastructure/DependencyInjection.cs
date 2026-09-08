using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Abstractions.Persistence;
using Pedidos.Application.Abstractions.Security;
using Pedidos.Domain.Repositories;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Persistence.Repositories;
using Pedidos.Infrastructure.Resiliencia;
using Pedidos.Infrastructure.Security;

namespace Pedidos.Infrastructure;

public static class DependencyInjection
{
    private const string NombreDeLaConexion = "Postgres";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddResiliencia(configuration);
        services.AddPersistencia(configuration);
        services.AddSeguridad(configuration);

        return services;
    }

    public static JwtOptions LeerOpcionesJwt(IConfiguration configuration)
    {
        var opciones = configuration
            .GetSection(JwtOptions.SeccionDeConfiguracion)
            .Get<JwtOptions>();

        if (opciones is null)
        {
            throw new InvalidOperationException(
                $"Falta la sección de configuración '{JwtOptions.SeccionDeConfiguracion}'.");
        }

        if (string.IsNullOrWhiteSpace(opciones.Issuer) || string.IsNullOrWhiteSpace(opciones.Audience))
        {
            throw new InvalidOperationException(
                $"'{JwtOptions.SeccionDeConfiguracion}:Issuer' y '{JwtOptions.SeccionDeConfiguracion}:Audience' son obligatorios.");
        }

        if (Encoding.UTF8.GetByteCount(opciones.Key) < JwtOptions.LongitudMinimaDeClaveEnBytes)
        {
            throw new InvalidOperationException(
                $"'{JwtOptions.SeccionDeConfiguracion}:Key' debe tener al menos " +
                $"{JwtOptions.LongitudMinimaDeClaveEnBytes} bytes para firmar con HMAC-SHA256. " +
                "Defínela en la variable de entorno Jwt__Key o en los secretos de usuario.");
        }

        if (opciones.ExpiracionEnMinutos <= 0)
        {
            throw new InvalidOperationException(
                $"'{JwtOptions.SeccionDeConfiguracion}:ExpiracionEnMinutos' debe ser mayor que cero.");
        }

        return opciones;
    }

    private static void AddResiliencia(this IServiceCollection services, IConfiguration configuration)
    {
        var opciones = configuration
            .GetSection(OpcionesDeResiliencia.SeccionDeConfiguracion)
            .Get<OpcionesDeResiliencia>() ?? new OpcionesDeResiliencia();

        services.AddPipelinesDeBaseDeDatos(opciones);
        services.AddScoped<IEjecutorDeBaseDeDatos, EjecutorDeBaseDeDatos>();
    }

    private static void AddPersistencia(this IServiceCollection services, IConfiguration configuration)
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
                npgsql.MigrationsAssembly(typeof(PedidosDbContext).Assembly.GetName().Name)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<DatabaseInitializer>();
    }

    private static void AddSeguridad(this IServiceCollection services, IConfiguration configuration)
    {
        LeerOpcionesJwt(configuration);

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SeccionDeConfiguracion));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IGeneradorDeToken, GeneradorDeTokenJwt>();
    }
}
