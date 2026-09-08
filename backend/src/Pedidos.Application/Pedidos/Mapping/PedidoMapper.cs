using Pedidos.Application.Pedidos.Dtos;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Application.Pedidos.Mapping;

internal static class PedidoMapper
{
    public static PedidoDto ADto(this Pedido pedido) => new(
        pedido.Id,
        pedido.NumeroPedido,
        pedido.Cliente,
        pedido.Fecha,
        pedido.Total,
        pedido.Estado.ToString());

    public static EstadoPedido AEstadoPedido(string estado)
    {
        if (Enum.TryParse<EstadoPedido>(estado, ignoreCase: true, out var resultado)
            && Enum.IsDefined(resultado))
        {
            return resultado;
        }

        var validos = string.Join(", ", Enum.GetNames<EstadoPedido>());
        throw new ReglaDeNegocioException($"El estado '{estado}' no es válido. Valores permitidos: {validos}.");
    }
}
