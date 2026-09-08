using Pedidos.Application.Pedidos.Dtos;
using Pedidos.Application.Pedidos.UseCases;
using Pedidos.Application.Tests.Dobles;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Application.Tests.Pedidos;

public class CrearPedidoUseCaseTests
{
    private readonly PedidoRepositoryEnMemoria _repositorio = new();
    private readonly UnitOfWorkEspia _unitOfWork = new();
    private readonly CrearPedidoUseCase _useCase;

    public CrearPedidoUseCaseTests()
    {
        _useCase = new CrearPedidoUseCase(_repositorio, _unitOfWork);
    }

    private static CrearPedidoRequest Solicitud(
        string numeroPedido = "PED-100",
        string cliente = "Juan Perez",
        decimal total = 250.75m) =>
        new(numeroPedido, cliente, new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc), total);

    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_PersisteYDevuelveElPedido()
    {
        var resultado = await _useCase.EjecutarAsync(Solicitud());

        Assert.Equal("PED-100", resultado.NumeroPedido);
        Assert.Equal("Registrado", resultado.Estado);
        Assert.Equal(250.75m, resultado.Total);
        Assert.Equal(1, _repositorio.VecesQueSeLlamoAgregar);
        Assert.Equal(1, _unitOfWork.VecesQueSeGuardo);
    }

    [Fact]
    public async Task EjecutarAsync_ConNumeroDePedidoRepetido_LanzaDuplicadoYNoGuarda()
    {
        _repositorio.Sembrar(Pedido.Crear("PED-100", "Ana Diaz", DateTime.UtcNow, 10m));

        await Assert.ThrowsAsync<NumeroPedidoDuplicadoException>(() => _useCase.EjecutarAsync(Solicitud()));

        Assert.Equal(0, _unitOfWork.VecesQueSeGuardo);
    }

    [Fact]
    public async Task EjecutarAsync_DetectaDuplicadosSinImportarMayusculasNiEspacios()
    {
        _repositorio.Sembrar(Pedido.Crear("PED-100", "Ana Diaz", DateTime.UtcNow, 10m));

        await Assert.ThrowsAsync<NumeroPedidoDuplicadoException>(
            () => _useCase.EjecutarAsync(Solicitud(numeroPedido: "  ped-100  ")));
    }

    [Fact]
    public async Task EjecutarAsync_ConNumeroDeUnPedidoEliminado_PermiteReutilizarlo()
    {
        var eliminado = _repositorio.Sembrar(Pedido.Crear("PED-100", "Ana Diaz", DateTime.UtcNow, 10m));
        eliminado.Eliminar();

        var resultado = await _useCase.EjecutarAsync(Solicitud());

        Assert.Equal("PED-100", resultado.NumeroPedido);
        Assert.Equal(1, _unitOfWork.VecesQueSeGuardo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task EjecutarAsync_ConTotalInvalido_LanzaReglaDeNegocioYNoConsultaDuplicados(decimal total)
    {
        await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => _useCase.EjecutarAsync(Solicitud(total: total)));

        Assert.Equal(0, _repositorio.VecesQueSeLlamoAgregar);
        Assert.Equal(0, _unitOfWork.VecesQueSeGuardo);
    }
}
