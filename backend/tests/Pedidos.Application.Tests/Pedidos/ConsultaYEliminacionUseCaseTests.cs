using Pedidos.Application.Pedidos.UseCases;
using Pedidos.Application.Tests.Dobles;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Application.Tests.Pedidos;

public class ConsultaYEliminacionUseCaseTests
{
    private readonly PedidoRepositoryEnMemoria _repositorio = new();
    private readonly UnitOfWorkEspia _unitOfWork = new();

    [Fact]
    public async Task ObtenerPedidos_DevuelveTodosLosPedidosActivos()
    {
        _repositorio.Sembrar(Pedido.Crear("PED-001", "Juan Perez", DateTime.UtcNow, 10m));
        _repositorio.Sembrar(Pedido.Crear("PED-002", "Ana Diaz", DateTime.UtcNow, 20m));

        var resultado = await new ObtenerPedidosUseCase(_repositorio).EjecutarAsync();

        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task ObtenerPedidos_NoDevuelveLosEliminados()
    {
        _repositorio.Sembrar(Pedido.Crear("PED-001", "Juan Perez", DateTime.UtcNow, 10m));
        var eliminado = _repositorio.Sembrar(Pedido.Crear("PED-002", "Ana Diaz", DateTime.UtcNow, 20m));
        eliminado.Eliminar();

        var resultado = await new ObtenerPedidosUseCase(_repositorio).EjecutarAsync();

        Assert.Single(resultado);
        Assert.Equal("PED-001", resultado[0].NumeroPedido);
    }

    [Fact]
    public async Task ObtenerPedidoPorId_ConIdExistente_DevuelveElPedido()
    {
        var pedido = _repositorio.Sembrar(Pedido.Crear("PED-001", "Juan Perez", DateTime.UtcNow, 250.75m));

        var resultado = await new ObtenerPedidoPorIdUseCase(_repositorio).EjecutarAsync(pedido.Id);

        Assert.Equal(pedido.Id, resultado.Id);
        Assert.Equal("PED-001", resultado.NumeroPedido);
        Assert.Equal(250.75m, resultado.Total);
    }

    [Fact]
    public async Task ObtenerPedidoPorId_ConIdInexistente_LanzaNoEncontrado()
    {
        var excepcion = await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => new ObtenerPedidoPorIdUseCase(_repositorio).EjecutarAsync(999));

        Assert.Equal("Pedido", excepcion.Recurso);
        Assert.Equal(999, excepcion.Identificador);
    }

    [Fact]
    public async Task EliminarPedido_MarcaElPedidoComoEliminadoYGuarda()
    {
        var pedido = _repositorio.Sembrar(Pedido.Crear("PED-001", "Juan Perez", DateTime.UtcNow, 10m));

        await new EliminarPedidoUseCase(_repositorio, _unitOfWork).EjecutarAsync(pedido.Id);

        Assert.True(pedido.Eliminado);
        Assert.NotNull(pedido.FechaEliminacion);
        Assert.Equal(1, _repositorio.VecesQueSeLlamoActualizar);
        Assert.Equal(1, _unitOfWork.VecesQueSeGuardo);
    }

    [Fact]
    public async Task EliminarPedido_ConIdInexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => new EliminarPedidoUseCase(_repositorio, _unitOfWork).EjecutarAsync(999));

        Assert.Equal(0, _unitOfWork.VecesQueSeGuardo);
    }

    [Fact]
    public async Task EliminarPedido_ConUnPedidoYaEliminado_LanzaNoEncontrado()
    {
        var pedido = _repositorio.Sembrar(Pedido.Crear("PED-001", "Juan Perez", DateTime.UtcNow, 10m));
        pedido.Eliminar();

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => new EliminarPedidoUseCase(_repositorio, _unitOfWork).EjecutarAsync(pedido.Id));
    }
}
