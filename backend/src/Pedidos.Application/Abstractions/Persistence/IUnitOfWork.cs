namespace Pedidos.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
