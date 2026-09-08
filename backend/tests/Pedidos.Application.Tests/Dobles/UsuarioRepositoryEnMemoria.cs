using Pedidos.Domain.Entities;
using Pedidos.Domain.Repositories;

namespace Pedidos.Application.Tests.Dobles;

internal sealed class UsuarioRepositoryEnMemoria : IUsuarioRepository
{
    private readonly List<Usuario> _usuarios = [];

    public UsuarioRepositoryEnMemoria Sembrar(Usuario usuario)
    {
        _usuarios.Add(usuario);
        return this;
    }

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_usuarios.FirstOrDefault(usuario => usuario.Id == id));

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizado = email.Trim().ToLowerInvariant();

        return Task.FromResult(_usuarios.FirstOrDefault(usuario => usuario.Email == normalizado));
    }
}
