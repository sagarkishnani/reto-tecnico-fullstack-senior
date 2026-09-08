using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Pedidos.Infrastructure.Resiliencia;

internal static class PipelinesDeBaseDeDatos
{
    public const string Lectura = "base-de-datos-lectura";
    public const string Escritura = "base-de-datos-escritura";

    public static IServiceCollection AddPipelinesDeBaseDeDatos(
        this IServiceCollection services,
        OpcionesDeResiliencia opciones)
    {
        services.AddResiliencePipeline(Lectura, (builder, contexto) =>
        {
            var logger = ObtenerLogger(contexto.ServiceProvider);

            if (opciones.ReintentosMaximos > 0)
            {
                builder.AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(EsFalloTransitorio),
                    MaxRetryAttempts = opciones.ReintentosMaximos,
                    Delay = TimeSpan.FromMilliseconds(opciones.RetardoBaseEnMilisegundos),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    OnRetry = argumentos =>
                    {
                        logger.LogWarning(
                            argumentos.Outcome.Exception,
                            "Reintento {Intento} de lectura en base de datos tras {Retardo}.",
                            argumentos.AttemptNumber + 1,
                            argumentos.RetryDelay);
                        return default;
                    }
                });
            }

            builder
                .AddCircuitBreaker(ConstruirCircuitBreaker(opciones, logger))
                .AddTimeout(TimeSpan.FromSeconds(opciones.TimeoutDeLecturaEnSegundos));
        });

        services.AddResiliencePipeline(Escritura, (builder, contexto) =>
        {
            var logger = ObtenerLogger(contexto.ServiceProvider);

            builder
                .AddCircuitBreaker(ConstruirCircuitBreaker(opciones, logger))
                .AddTimeout(TimeSpan.FromSeconds(opciones.TimeoutDeEscrituraEnSegundos));
        });

        return services;
    }

    private static CircuitBreakerStrategyOptions ConstruirCircuitBreaker(
        OpcionesDeResiliencia opciones,
        ILogger logger) =>
        new()
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(EsFalloTransitorio),
            FailureRatio = opciones.ProporcionDeFallosParaAbrir,
            MinimumThroughput = opciones.MuestrasMinimasParaAbrir,
            SamplingDuration = TimeSpan.FromSeconds(opciones.VentanaDeMuestreoEnSegundos),
            BreakDuration = TimeSpan.FromSeconds(opciones.DuracionDeAperturaEnSegundos),
            OnOpened = argumentos =>
            {
                logger.LogError(
                    argumentos.Outcome.Exception,
                    "Circuito de base de datos ABIERTO por {Duracion}.",
                    argumentos.BreakDuration);
                return default;
            },
            OnClosed = _ =>
            {
                logger.LogInformation("Circuito de base de datos CERRADO, servicio restablecido.");
                return default;
            }
        };

    private static bool EsFalloTransitorio(Exception excepcion) => excepcion switch
    {
        NpgsqlException npgsql => npgsql.IsTransient,
        TimeoutRejectedException => true,
        _ => false
    };

    private static ILogger ObtenerLogger(IServiceProvider proveedor) =>
        proveedor.GetRequiredService<ILoggerFactory>().CreateLogger("Pedidos.Resiliencia");
}
