using Pedidos.Application.Abstractions.Persistence;

namespace Pedidos.Application.Tests.Dobles;

internal sealed class UnitOfWorkEspia : IUnitOfWork
{
    public int VecesQueSeGuardo { get; private set; }

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        VecesQueSeGuardo++;
        return Task.FromResult(1);
    }
}
