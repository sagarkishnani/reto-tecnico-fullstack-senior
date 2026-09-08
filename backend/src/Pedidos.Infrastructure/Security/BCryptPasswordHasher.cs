using Pedidos.Application.Abstractions.Security;

namespace Pedidos.Infrastructure.Security;

internal sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int FactorDeTrabajo = 12;

    public string Hashear(string password) => BCrypt.Net.BCrypt.HashPassword(password, FactorDeTrabajo);

    public bool Verificar(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
