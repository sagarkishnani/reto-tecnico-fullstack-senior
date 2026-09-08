using Pedidos.Application.Abstractions.Security;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Tests.Dobles;

internal sealed class GeneradorDeTokenFalso : IGeneradorDeToken
{
    public Usuario? UsuarioRecibido { get; private set; }

    public TokenGenerado Generar(Usuario usuario)
    {
        UsuarioRecibido = usuario;
        return new TokenGenerado($"token-de-{usuario.Email}", 3600);
    }
}
