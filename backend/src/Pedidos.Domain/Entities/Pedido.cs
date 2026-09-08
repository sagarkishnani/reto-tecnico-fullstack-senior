using Pedidos.Domain.Common;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Domain.Entities;

public sealed class Pedido : EntidadBase
{
    public const int LongitudMaximaNumeroPedido = 50;
    public const int LongitudMaximaCliente = 150;
    public const decimal TotalMaximo = 99_999_999.99m;

    private Pedido()
    {
    }

    public string NumeroPedido { get; private set; } = string.Empty;

    public string Cliente { get; private set; } = string.Empty;

    public DateTime Fecha { get; private set; }

    public decimal Total { get; private set; }

    public EstadoPedido Estado { get; private set; }

    public static Pedido Crear(string numeroPedido, string cliente, DateTime fecha, decimal total)
    {
        var pedido = new Pedido
        {
            NumeroPedido = NormalizarNumeroPedido(numeroPedido),
            Cliente = NormalizarCliente(cliente),
            Fecha = NormalizarFecha(fecha),
            Total = NormalizarTotal(total),
            Estado = EstadoPedido.Registrado
        };

        pedido.RegistrarCreacion();
        return pedido;
    }

    public void Actualizar(string numeroPedido, string cliente, DateTime fecha, decimal total, EstadoPedido estado)
    {
        GarantizarQueNoEsteEliminado();

        NumeroPedido = NormalizarNumeroPedido(numeroPedido);
        Cliente = NormalizarCliente(cliente);
        Fecha = NormalizarFecha(fecha);
        Total = NormalizarTotal(total);
        Estado = estado;

        RegistrarActualizacion();
    }

    public void CambiarEstado(EstadoPedido estado)
    {
        GarantizarQueNoEsteEliminado();

        if (Estado == estado)
        {
            return;
        }

        Estado = estado;
        RegistrarActualizacion();
    }

    private void GarantizarQueNoEsteEliminado()
    {
        if (Eliminado)
        {
            throw new ReglaDeNegocioException("No se puede modificar un pedido eliminado.");
        }
    }

    private static string NormalizarNumeroPedido(string numeroPedido)
    {
        if (string.IsNullOrWhiteSpace(numeroPedido))
        {
            throw new ReglaDeNegocioException("El número de pedido es obligatorio.");
        }

        var normalizado = numeroPedido.Trim().ToUpperInvariant();

        if (normalizado.Length > LongitudMaximaNumeroPedido)
        {
            throw new ReglaDeNegocioException(
                $"El número de pedido no puede superar los {LongitudMaximaNumeroPedido} caracteres.");
        }

        return normalizado;
    }

    private static string NormalizarCliente(string cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente))
        {
            throw new ReglaDeNegocioException("El cliente es obligatorio.");
        }

        var normalizado = cliente.Trim();

        if (normalizado.Length > LongitudMaximaCliente)
        {
            throw new ReglaDeNegocioException(
                $"El cliente no puede superar los {LongitudMaximaCliente} caracteres.");
        }

        return normalizado;
    }

    private static DateTime NormalizarFecha(DateTime fecha)
    {
        if (fecha == default)
        {
            throw new ReglaDeNegocioException("La fecha del pedido es obligatoria.");
        }

        return fecha.Kind switch
        {
            DateTimeKind.Utc => fecha,
            DateTimeKind.Local => fecha.ToUniversalTime(),
            _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
        };
    }

    private static decimal NormalizarTotal(decimal total)
    {
        if (total <= 0)
        {
            throw new ReglaDeNegocioException("El total del pedido debe ser mayor que cero.");
        }

        if (total > TotalMaximo)
        {
            throw new ReglaDeNegocioException($"El total del pedido no puede superar {TotalMaximo:N2}.");
        }

        return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
