using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pedidos.Infrastructure.Resiliencia;
using Polly.Registry;

namespace Pedidos.Infrastructure.Tests.Resiliencia;

public class EjecutorDeBaseDeDatosTests
{
    private static NpgsqlException FalloTransitorio() =>
        new("conexión perdida", new TimeoutException());

    private static IEjecutorDeBaseDeDatos ConstruirEjecutor(OpcionesDeResiliencia opciones)
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging(constructor => constructor.SetMinimumLevel(LogLevel.None));
        servicios.AddPipelinesDeBaseDeDatos(opciones);

        var proveedor = servicios.BuildServiceProvider();

        return new EjecutorDeBaseDeDatos(proveedor.GetRequiredService<ResiliencePipelineProvider<string>>());
    }

    private static OpcionesDeResiliencia OpcionesRapidas() => new()
    {
        ReintentosMaximos = 3,
        RetardoBaseEnMilisegundos = 1,
        TimeoutDeLecturaEnSegundos = 5,
        TimeoutDeEscrituraEnSegundos = 5,
        ProporcionDeFallosParaAbrir = 0.5,
        MuestrasMinimasParaAbrir = 2,
        VentanaDeMuestreoEnSegundos = 30,
        DuracionDeAperturaEnSegundos = 30
    };

    [Fact]
    public async Task LeerAsync_ConFalloTransitorioQueLuegoSeRecupera_ReintentaHastaTenerExito()
    {
        var opciones = OpcionesRapidas();
        opciones.MuestrasMinimasParaAbrir = 100;
        var ejecutor = ConstruirEjecutor(opciones);
        var intentos = 0;

        var resultado = await ejecutor.LeerAsync(_ =>
        {
            intentos++;
            return intentos < 3 ? throw FalloTransitorio() : Task.FromResult("ok");
        });

        Assert.Equal("ok", resultado);
        Assert.Equal(3, intentos);
    }

    [Fact]
    public async Task LeerAsync_ConFalloNoTransitorio_NoReintenta()
    {
        var ejecutor = ConstruirEjecutor(OpcionesRapidas());
        var intentos = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() => ejecutor.LeerAsync<string>(_ =>
        {
            intentos++;
            throw new InvalidOperationException("error de negocio, no de infraestructura");
        }));

        Assert.Equal(1, intentos);
    }

    [Fact]
    public async Task LeerAsync_AgotadosLosReintentos_PropagaLaExcepcionOriginal()
    {
        var opciones = OpcionesRapidas();
        opciones.MuestrasMinimasParaAbrir = 100;
        var ejecutor = ConstruirEjecutor(opciones);
        var intentos = 0;

        await Assert.ThrowsAsync<NpgsqlException>(() => ejecutor.LeerAsync<string>(_ =>
        {
            intentos++;
            throw FalloTransitorio();
        }));

        Assert.Equal(opciones.ReintentosMaximos + 1, intentos);
    }

    [Fact]
    public async Task LeerAsync_TrasFallosSostenidos_AbreElCircuitoYFallaRapido()
    {
        var ejecutor = ConstruirEjecutor(OpcionesRapidas());

        for (var i = 0; i < 3; i++)
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => ejecutor.LeerAsync<string>(_ => throw FalloTransitorio()));
        }

        var intentosTrasAbrir = 0;

        var excepcion = await Assert.ThrowsAsync<BaseDeDatosNoDisponibleException>(
            () => ejecutor.LeerAsync<string>(_ =>
            {
                intentosTrasAbrir++;
                throw FalloTransitorio();
            }));

        Assert.Equal(0, intentosTrasAbrir);
        Assert.Contains("circuito", excepcion.Message);
    }

    [Fact]
    public async Task EscribirAsync_ConFalloTransitorio_NoReintentaPorqueLaEscrituraNoEsIdempotente()
    {
        var ejecutor = ConstruirEjecutor(OpcionesRapidas());
        var intentos = 0;

        await Assert.ThrowsAsync<NpgsqlException>(() => ejecutor.EscribirAsync<int>(_ =>
        {
            intentos++;
            throw FalloTransitorio();
        }));

        Assert.Equal(1, intentos);
    }

    [Fact]
    public async Task ConfigurarConCeroReintentos_NoRompeElArranque()
    {
        var opciones = OpcionesRapidas();
        opciones.ReintentosMaximos = 0;
        opciones.MuestrasMinimasParaAbrir = 100;

        var ejecutor = ConstruirEjecutor(opciones);
        var intentos = 0;

        await Assert.ThrowsAsync<NpgsqlException>(() => ejecutor.LeerAsync<string>(_ =>
        {
            intentos++;
            throw FalloTransitorio();
        }));

        Assert.Equal(1, intentos);
    }

    [Fact]
    public async Task EscribirAsync_ConOperacionCorrecta_DevuelveElResultado()
    {
        var ejecutor = ConstruirEjecutor(OpcionesRapidas());

        var filas = await ejecutor.EscribirAsync(_ => Task.FromResult(1));

        Assert.Equal(1, filas);
    }

    [Fact]
    public async Task LeerAsync_CuandoSeSuperaElTimeout_LoTraduceAServicioNoDisponible()
    {
        var opciones = OpcionesRapidas();
        opciones.ReintentosMaximos = 0;
        opciones.TimeoutDeLecturaEnSegundos = 1;
        opciones.MuestrasMinimasParaAbrir = 100;
        var ejecutor = ConstruirEjecutor(opciones);

        var excepcion = await Assert.ThrowsAsync<BaseDeDatosNoDisponibleException>(
            () => ejecutor.LeerAsync<string>(async token =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
                return "nunca llega";
            }));

        Assert.Contains("tiempo máximo", excepcion.Message);
    }
}
