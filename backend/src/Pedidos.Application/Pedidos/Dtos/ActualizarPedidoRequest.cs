namespace Pedidos.Application.Pedidos.Dtos;

public sealed record ActualizarPedidoRequest(
    string NumeroPedido,
    string Cliente,
    DateTime Fecha,
    decimal Total,
    string Estado);
