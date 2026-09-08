using Pedidos.Domain.Entities;

namespace Pedidos.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
}
