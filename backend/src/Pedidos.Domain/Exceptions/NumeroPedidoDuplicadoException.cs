namespace Pedidos.Domain.Exceptions;

public sealed class NumeroPedidoDuplicadoException : DomainException
{
    public NumeroPedidoDuplicadoException(string numeroPedido)
        : base($"Ya existe un pedido con el número {numeroPedido}.")
    {
        NumeroPedido = numeroPedido;
    }

    public string NumeroPedido { get; }
}
