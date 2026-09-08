namespace Pedidos.Application.Pedidos.Dtos;

public sealed record PedidoDto(
    int Id,
    string NumeroPedido,
    string Cliente,
    DateTime Fecha,
    decimal Total,
    string Estado);
