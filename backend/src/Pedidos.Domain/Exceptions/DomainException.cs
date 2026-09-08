namespace Pedidos.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string mensaje) : base(mensaje)
    {
    }
}
