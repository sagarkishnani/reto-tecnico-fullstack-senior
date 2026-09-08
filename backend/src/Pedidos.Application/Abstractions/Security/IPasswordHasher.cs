namespace Pedidos.Application.Abstractions.Security;

public interface IPasswordHasher
{
    string Hashear(string password);

    bool Verificar(string password, string hash);
}
