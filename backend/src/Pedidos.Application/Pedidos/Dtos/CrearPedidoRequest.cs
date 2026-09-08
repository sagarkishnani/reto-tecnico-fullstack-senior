namespace Pedidos.Application.Pedidos.Dtos;

public sealed record CrearPedidoRequest(
    string NumeroPedido,
    string Cliente,
    DateTime Fecha,
    decimal Total);
