using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;
using Pedidos.Domain.Tests.Builders;

namespace Pedidos.Domain.Tests.Entities;

public class PedidoTests
{
    [Fact]
    public void Crear_ConDatosValidos_DevuelvePedidoEnEstadoRegistrado()
    {
        var pedido = new PedidoBuilder().Construir();

        Assert.Equal("PED-001", pedido.NumeroPedido);
        Assert.Equal("Juan Perez", pedido.Cliente);
        Assert.Equal(250.75m, pedido.Total);
        Assert.Equal(EstadoPedido.Registrado, pedido.Estado);
        Assert.False(pedido.Eliminado);
        Assert.Null(pedido.FechaActualizacion);
        Assert.NotEqual(default, pedido.FechaCreacion);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-250.75)]
    public void Crear_ConTotalMenorOIgualACero_LanzaReglaDeNegocio(decimal total)
    {
        var excepcion = Assert.Throws<ReglaDeNegocioException>(
            () => new PedidoBuilder().ConTotal(total).Construir());

        Assert.Contains("mayor que cero", excepcion.Message);
    }

    [Fact]
    public void Crear_ConTotalSobreElMaximoDeLaColumna_LanzaReglaDeNegocio()
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new PedidoBuilder().ConTotal(Pedido.TotalMaximo + 0.01m).Construir());
    }

    [Theory]
    [InlineData(10.126, 10.13)]
    [InlineData(10.124, 10.12)]
    [InlineData(10.125, 10.13)]
    public void Crear_ConMasDeDosDecimales_RedondeaAlPrecisionDeLaColumna(decimal total, decimal esperado)
    {
        var pedido = new PedidoBuilder().ConTotal(total).Construir();

        Assert.Equal(esperado, pedido.Total);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_ConNumeroPedidoVacio_LanzaReglaDeNegocio(string? numeroPedido)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new PedidoBuilder().ConNumeroPedido(numeroPedido!).Construir());
    }

    [Theory]
    [InlineData("ped-001", "PED-001")]
    [InlineData("  PED-002  ", "PED-002")]
    [InlineData("Ped-003", "PED-003")]
    public void Crear_NormalizaElNumeroDePedido(string entrada, string esperado)
    {
        var pedido = new PedidoBuilder().ConNumeroPedido(entrada).Construir();

        Assert.Equal(esperado, pedido.NumeroPedido);
    }

    [Fact]
    public void Crear_ConNumeroPedidoQueExcedeLaLongitudMaxima_LanzaReglaDeNegocio()
    {
        var numeroLargo = new string('X', Pedido.LongitudMaximaNumeroPedido + 1);

        Assert.Throws<ReglaDeNegocioException>(
            () => new PedidoBuilder().ConNumeroPedido(numeroLargo).Construir());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_ConClienteVacio_LanzaReglaDeNegocio(string? cliente)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new PedidoBuilder().ConCliente(cliente!).Construir());
    }

    [Fact]
    public void Crear_ConClienteQueExcedeLaLongitudMaxima_LanzaReglaDeNegocio()
    {
        var clienteLargo = new string('X', Pedido.LongitudMaximaCliente + 1);

        Assert.Throws<ReglaDeNegocioException>(
            () => new PedidoBuilder().ConCliente(clienteLargo).Construir());
    }

    [Fact]
    public void Crear_ConFechaSinKind_LaMarcaComoUtcSinDesplazarLaHora()
    {
        var fecha = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Unspecified);

        var pedido = new PedidoBuilder().ConFecha(fecha).Construir();

        Assert.Equal(DateTimeKind.Utc, pedido.Fecha.Kind);
        Assert.Equal(new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc), pedido.Fecha);
    }

    [Fact]
    public void Crear_ConFechaLocal_LaConvierteAUtc()
    {
        var fecha = new DateTime(2025, 1, 10, 15, 30, 0, DateTimeKind.Local);

        var pedido = new PedidoBuilder().ConFecha(fecha).Construir();

        Assert.Equal(DateTimeKind.Utc, pedido.Fecha.Kind);
        Assert.Equal(fecha.ToUniversalTime(), pedido.Fecha);
    }

    [Fact]
    public void Crear_ConFechaPorDefecto_LanzaReglaDeNegocio()
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new PedidoBuilder().ConFecha(default).Construir());
    }

    [Fact]
    public void Actualizar_ConDatosValidos_ReemplazaLosCamposYRegistraLaFecha()
    {
        var pedido = new PedidoBuilder().Construir();

        pedido.Actualizar("ped-999", "  Ana Diaz  ", new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc), 99.999m, EstadoPedido.Enviado);

        Assert.Equal("PED-999", pedido.NumeroPedido);
        Assert.Equal("Ana Diaz", pedido.Cliente);
        Assert.Equal(100.00m, pedido.Total);
        Assert.Equal(EstadoPedido.Enviado, pedido.Estado);
        Assert.NotNull(pedido.FechaActualizacion);
    }

    [Fact]
    public void Actualizar_ConTotalCero_LanzaReglaDeNegocio()
    {
        var pedido = new PedidoBuilder().Construir();

        Assert.Throws<ReglaDeNegocioException>(
            () => pedido.Actualizar("PED-001", "Juan Perez", DateTime.UtcNow, 0m, EstadoPedido.Registrado));
    }

    [Fact]
    public void Actualizar_SobreUnPedidoEliminado_LanzaReglaDeNegocio()
    {
        var pedido = new PedidoBuilder().Construir();
        pedido.Eliminar();

        var excepcion = Assert.Throws<ReglaDeNegocioException>(
            () => pedido.Actualizar("PED-002", "Ana Diaz", DateTime.UtcNow, 10m, EstadoPedido.Enviado));

        Assert.Contains("eliminado", excepcion.Message);
    }

    [Fact]
    public void CambiarEstado_ConUnEstadoDistinto_LoActualizaYRegistraLaFecha()
    {
        var pedido = new PedidoBuilder().Construir();

        pedido.CambiarEstado(EstadoPedido.Entregado);

        Assert.Equal(EstadoPedido.Entregado, pedido.Estado);
        Assert.NotNull(pedido.FechaActualizacion);
    }

    [Fact]
    public void CambiarEstado_ConElMismoEstado_NoRegistraActualizacion()
    {
        var pedido = new PedidoBuilder().Construir();

        pedido.CambiarEstado(EstadoPedido.Registrado);

        Assert.Null(pedido.FechaActualizacion);
    }

    [Fact]
    public void CambiarEstado_SobreUnPedidoEliminado_LanzaReglaDeNegocio()
    {
        var pedido = new PedidoBuilder().Construir();
        pedido.Eliminar();

        Assert.Throws<ReglaDeNegocioException>(() => pedido.CambiarEstado(EstadoPedido.Cancelado));
    }

    [Fact]
    public void Eliminar_MarcaLaBanderaYRegistraLaFecha()
    {
        var pedido = new PedidoBuilder().Construir();

        pedido.Eliminar();

        Assert.True(pedido.Eliminado);
        Assert.NotNull(pedido.FechaEliminacion);
    }

    [Fact]
    public void Eliminar_EsIdempotente()
    {
        var pedido = new PedidoBuilder().Construir();
        pedido.Eliminar();
        var primeraFecha = pedido.FechaEliminacion;

        pedido.Eliminar();

        Assert.True(pedido.Eliminado);
        Assert.Equal(primeraFecha, pedido.FechaEliminacion);
    }
}
