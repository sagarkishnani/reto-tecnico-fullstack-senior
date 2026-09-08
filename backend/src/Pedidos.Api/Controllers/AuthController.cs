using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pedidos.Api.Extensions;
using Pedidos.Application.Autenticacion.Dtos;
using Pedidos.Application.Autenticacion.UseCases;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IIniciarSesionUseCase _iniciarSesion;

    public AuthController(IIniciarSesionUseCase iniciarSesion)
    {
        _iniciarSesion = iniciarSesion;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.PoliticaDeLogin)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var respuesta = await _iniciarSesion.EjecutarAsync(request, cancellationToken);

        return Ok(respuesta);
    }
}
