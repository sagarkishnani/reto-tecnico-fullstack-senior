namespace Pedidos.Domain.Exceptions;

public sealed class CredencialesInvalidasException : DomainException
{
    public CredencialesInvalidasException() : base("Las credenciales proporcionadas no son válidas.")
    {
    }
}
