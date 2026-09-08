using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Timeout;

namespace Pedidos.Infrastructure.Resiliencia;

internal sealed class EjecutorDeBaseDeDatos : IEjecutorDeBaseDeDatos
{
    private readonly ResiliencePipeline _lectura;
    private readonly ResiliencePipeline _escritura;

    public EjecutorDeBaseDeDatos(ResiliencePipelineProvider<string> proveedor)
    {
        _lectura = proveedor.GetPipeline(PipelinesDeBaseDeDatos.Lectura);
        _escritura = proveedor.GetPipeline(PipelinesDeBaseDeDatos.Escritura);
    }

    public Task<T> LeerAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default) =>
        EjecutarAsync(_lectura, operacion, cancellationToken);

    public Task<T> EscribirAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default) =>
        EjecutarAsync(_escritura, operacion, cancellationToken);

    private static async Task<T> EjecutarAsync<T>(
        ResiliencePipeline pipeline,
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken)
    {
        try
        {
            return await pipeline.ExecuteAsync(
                async token => await operacion(token),
                cancellationToken);
        }
        catch (BrokenCircuitException excepcion)
        {
            throw new BaseDeDatosNoDisponibleException(
                "La base de datos no está respondiendo. El circuito está abierto para evitar sobrecargarla.",
                excepcion);
        }
        catch (TimeoutRejectedException excepcion)
        {
            throw new BaseDeDatosNoDisponibleException(
                "La consulta a la base de datos superó el tiempo máximo de espera.",
                excepcion);
        }
    }
}
