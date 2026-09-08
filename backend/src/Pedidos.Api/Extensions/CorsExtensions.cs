namespace Pedidos.Api.Extensions;

internal static class CorsExtensions
{
    public const string PoliticaDelCliente = "cliente-web";

    private const string SeccionDeOrigenes = "Cors:OrigenesPermitidos";

    public static IServiceCollection AddCorsDelClienteWeb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origenes = configuration.GetSection(SeccionDeOrigenes).Get<string[]>() ?? [];

        services.AddCors(opciones =>
            opciones.AddPolicy(PoliticaDelCliente, politica => politica
                .WithOrigins(origenes)
                .AllowAnyHeader()
                .AllowAnyMethod()));

        return services;
    }
}
