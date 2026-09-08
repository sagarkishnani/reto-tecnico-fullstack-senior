using Pedidos.Application.Pedidos.Dtos;
using Pedidos.Application.Pedidos.UseCases;
using Pedidos.Application.Tests.Dobles;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Application.Tests.Pedidos;

public class ActualizarPedidoUseCaseTests
{
    private readonly PedidoRepositoryEnMemoria _repositorio = new();
    private readonly UnitOfWorkEspia _unitOfWork = new();
    private readonly ActualizarPedidoUseCase _useCase;

    public ActualizarPedidoUseCaseTests()
    {
        _useCase = new ActualizarPedidoUseCase(_repositorio, _unitOfWork);
    }

    private static ActualizarPedidoRequest Solicitud(
        string numeroPedido = "PED-100",
        string cliente = "Ana Diaz",
        decimal total = 500m,
        string estado = "Enviado") =>
        new(numeroPedido, cliente, new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), total, estado);

    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_ActualizaYGuarda()
    {
        var pedido = _repositorio.Sembrar(Pedido.Crear("PED-100", "Juan Perez", DateTime.UtcNow, 10m));

        var resultado = await _useCase.EjecutarAsync(pedido.Id, Solicitud());

        Assert.Equal("Ana Diaz", resultado.Cliente);
        Assert.Equal("Enviado", resultado.Estado);
        Assert.Equal(500m, resultado.Total);
        Assert.Equal(1, _repositorio.VecesQueSeLlamoActualizar);
        Assert.Equal(1, _unitOfWork.VecesQueSeGuardo);
    }

    [Fact]
    public async Task EjecutarAsync_ConIdInexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _useCase.EjecutarAsync(999, Solicitud()));

        Assert.Equal(0, _unitOfWork.VecesQueSeGuardo);
    }

    [Fact]
    public async Task EjecutarAsync_ConservandoSuPropioNumero_NoLoConsideraDuplicado()
    {
        var pedido = _repositorio.Sembrar(Pedido.Crear("PED-100", "Juan Perez", DateTime.UtcNow, 10m));

        var resultado = await _useCase.EjecutarAsync(pedido.Id, Solicitud(numeroPedido: "PED-100"));

        Assert.Equal("PED-100", resultado.NumeroPedido);
    }

    [Fact]
    public async Task EjecutarAsync_ConNumeroDeOtroPedido_LanzaDuplicado()
    {
        var pedido = _repositorio.Sembrar(Pedido.Crear("PED-100", "Juan Perez", DateTime.UtcNow, 10m));
        _repositorio.Sembrar(Pedido.Crear("PED-200", "Otro Cliente", DateTime.UtcNow, 10m));

        await Assert.ThrowsAsync<NumeroPedidoDuplicadoException>(
            () => _useCase.EjecutarAsync(pedido.Id, Solicitud(numeroPedido: "PED-200")));

        Assert.Equal(0, _unitOfWork.VecesQueSeGuardo);
    }

    [Fact]
    public async Task EjecutarAsync_ConEstadoDesconocido_LanzaReglaDeNegocioConLosValoresValidos()
    {
        var pedido = _repositorio.Sembrar(Pedido.Crear("PED-100", "Juan Perez", DateTime.UtcNow, 10m));

        var excepcion = await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => _useCase.EjecutarAsync(pedido.Id, Solicitud(estado: "Inventado")));

        Assert.Contains("Registrado", excepcion.Message);
        Assert.Equal(0, _unitOfWork.VecesQueSeGuardo);
    }

    [Theory]
    [InlineData("enviado")]
    [InlineData("ENVIADO")]
    public async Task EjecutarAsync_AceptaElEstadoSinImportarMayusculas(string estado)
    {
        var pedido = _repositorio.Sembrar(Pedido.Crear("PED-100", "Juan Perez", DateTime.UtcNow, 10m));

        var resultado = await _useCase.EjecutarAsync(pedido.Id, Solicitud(estado: estado));

        Assert.Equal("Enviado", resultado.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ConTotalCero_LanzaReglaDeNegocio()
    {
        var pedido = _repositorio.Sembrar(Pedido.Crear("PED-100", "Juan Perez", DateTime.UtcNow, 10m));

        await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => _useCase.EjecutarAsync(pedido.Id, Solicitud(total: 0m)));
    }
}
