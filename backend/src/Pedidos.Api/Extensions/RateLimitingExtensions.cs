using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Pedidos.Api.Extensions;

internal static class RateLimitingExtensions
{
    public const string PoliticaDeLogin = "login";

    public static IServiceCollection AddRateLimitingDePedidos(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var opciones = configuration
            .GetSection(OpcionesDeRateLimiting.SeccionDeConfiguracion)
            .Get<OpcionesDeRateLimiting>() ?? new OpcionesDeRateLimiting();

        services.AddRateLimiter(limitador =>
        {
            limitador.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limitador.AddPolicy(PoliticaDeLogin, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ClaveDeParticion(contexto),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = opciones.IntentosDeLoginPorVentana,
                        Window = TimeSpan.FromMinutes(opciones.VentanaDeLoginEnMinutos),
                        QueueLimit = 0
                    }));

            limitador.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ClaveDeParticion(contexto),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = opciones.PeticionesDeApiPorVentana,
                        Window = TimeSpan.FromMinutes(opciones.VentanaDeApiEnMinutos),
                        QueueLimit = 0
                    }));

            limitador.OnRejected = async (contexto, cancellationToken) =>
            {
                if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var reintentarTras))
                {
                    contexto.HttpContext.Response.Headers.RetryAfter =
                        ((int)reintentarTras.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                await contexto.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Demasiadas peticiones",
                    Detail = "Se superó el límite de peticiones permitidas. Intente nuevamente más tarde.",
                    Instance = $"{contexto.HttpContext.Request.Method} {contexto.HttpContext.Request.Path}"
                }, cancellationToken);
            };
        });

        return services;
    }

    private static string ClaveDeParticion(HttpContext contexto) =>
        contexto.User.Identity?.IsAuthenticated == true
            ? $"usuario:{contexto.User.FindFirst("sub")?.Value}"
            : $"ip:{contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida"}";
}

public sealed class OpcionesDeRateLimiting
{
    public const string SeccionDeConfiguracion = "RateLimiting";

    public int IntentosDeLoginPorVentana { get; set; } = 5;

    public int VentanaDeLoginEnMinutos { get; set; } = 1;

    public int PeticionesDeApiPorVentana { get; set; } = 100;

    public int VentanaDeApiEnMinutos { get; set; } = 1;
}
