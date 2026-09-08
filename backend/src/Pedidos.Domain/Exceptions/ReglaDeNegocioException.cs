namespace Pedidos.Domain.Exceptions;

public sealed class ReglaDeNegocioException : DomainException
{
    public ReglaDeNegocioException(string mensaje) : base(mensaje)
    {
    }
}
