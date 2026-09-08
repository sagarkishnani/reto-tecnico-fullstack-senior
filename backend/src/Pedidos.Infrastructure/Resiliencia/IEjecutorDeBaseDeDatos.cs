namespace Pedidos.Infrastructure.Resiliencia;

internal interface IEjecutorDeBaseDeDatos
{
    Task<T> LeerAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default);

    Task<T> EscribirAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default);
}
