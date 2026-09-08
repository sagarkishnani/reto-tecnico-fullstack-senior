using Pedidos.Application.Abstractions.Security;

namespace Pedidos.Application.Tests.Dobles;

internal sealed class PasswordHasherFalso : IPasswordHasher
{
    public int VecesQueSeVerifico { get; private set; }

    public string Hashear(string password) => $"hash::{password}";

    public bool Verificar(string password, string hash)
    {
        VecesQueSeVerifico++;
        return hash == $"hash::{password}";
    }
}
