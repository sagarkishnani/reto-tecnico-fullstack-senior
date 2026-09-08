namespace Pedidos.Infrastructure.Resiliencia;

public sealed class BaseDeDatosNoDisponibleException : Exception
{
    public BaseDeDatosNoDisponibleException(string mensaje, Exception? innerException = null)
        : base(mensaje, innerException)
    {
    }
}
