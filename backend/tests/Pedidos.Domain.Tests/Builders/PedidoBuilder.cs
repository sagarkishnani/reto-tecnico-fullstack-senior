using Pedidos.Domain.Entities;

namespace Pedidos.Domain.Tests.Builders;

internal sealed class PedidoBuilder
{
    private string _numeroPedido = "PED-001";
    private string _cliente = "Juan Perez";
    private DateTime _fecha = new(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);
    private decimal _total = 250.75m;

    public PedidoBuilder ConNumeroPedido(string numeroPedido)
    {
        _numeroPedido = numeroPedido;
        return this;
    }

    public PedidoBuilder ConCliente(string cliente)
    {
        _cliente = cliente;
        return this;
    }

    public PedidoBuilder ConFecha(DateTime fecha)
    {
        _fecha = fecha;
        return this;
    }

    public PedidoBuilder ConTotal(decimal total)
    {
        _total = total;
        return this;
    }

    public Pedido Construir() => Pedido.Crear(_numeroPedido, _cliente, _fecha, _total);
}
