using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pedidos.Application.Pedidos.Dtos;
using Pedidos.Application.Pedidos.UseCases;

namespace Pedidos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pedidos")]
public sealed class PedidosController : ControllerBase
{
    private readonly IObtenerPedidosUseCase _obtenerPedidos;
    private readonly IObtenerPedidoPorIdUseCase _obtenerPedidoPorId;
    private readonly ICrearPedidoUseCase _crearPedido;
    private readonly IActualizarPedidoUseCase _actualizarPedido;
    private readonly IEliminarPedidoUseCase _eliminarPedido;

    public PedidosController(
        IObtenerPedidosUseCase obtenerPedidos,
        IObtenerPedidoPorIdUseCase obtenerPedidoPorId,
        ICrearPedidoUseCase crearPedido,
        IActualizarPedidoUseCase actualizarPedido,
        IEliminarPedidoUseCase eliminarPedido)
    {
        _obtenerPedidos = obtenerPedidos;
        _obtenerPedidoPorId = obtenerPedidoPorId;
        _crearPedido = crearPedido;
        _actualizarPedido = actualizarPedido;
        _eliminarPedido = eliminarPedido;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PedidoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PedidoDto>>> Obtener(CancellationToken cancellationToken)
    {
        var pedidos = await _obtenerPedidos.EjecutarAsync(cancellationToken);

        return Ok(pedidos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PedidoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var pedido = await _obtenerPedidoPorId.EjecutarAsync(id, cancellationToken);

        return Ok(pedido);
    }

    [HttpPost]
    [ProducesResponseType<PedidoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoDto>> Crear(
        [FromBody] CrearPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var pedido = await _crearPedido.EjecutarAsync(request, cancellationToken);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<PedidoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoDto>> Actualizar(
        int id,
        [FromBody] ActualizarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var pedido = await _actualizarPedido.EjecutarAsync(id, request, cancellationToken);

        return Ok(pedido);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _eliminarPedido.EjecutarAsync(id, cancellationToken);

        return NoContent();
    }
}
