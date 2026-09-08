using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Api.Errores;

internal sealed class ManejadorGlobalDeExcepciones : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ManejadorGlobalDeExcepciones> _logger;

    public ManejadorGlobalDeExcepciones(
        IProblemDetailsService problemDetailsService,
        ILogger<ManejadorGlobalDeExcepciones> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (estado, titulo, detalle) = Traducir(exception);

        if (estado == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Error no controlado en {Metodo} {Ruta}.",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning("{Titulo} en {Metodo} {Ruta}: {Detalle}",
                titulo, httpContext.Request.Method, httpContext.Request.Path, detalle);
        }

        httpContext.Response.StatusCode = estado;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = estado,
                Title = titulo,
                Detail = detalle,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            }
        });
    }

    private static (int Estado, string Titulo, string Detalle) Traducir(Exception exception) => exception switch
    {
        CredencialesInvalidasException => (
            StatusCodes.Status401Unauthorized,
            "Credenciales inválidas",
            exception.Message),

        RecursoNoEncontradoException => (
            StatusCodes.Status404NotFound,
            "Recurso no encontrado",
            exception.Message),

        NumeroPedidoDuplicadoException => (
            StatusCodes.Status409Conflict,
            "Conflicto de unicidad",
            exception.Message),

        DomainException => (
            StatusCodes.Status400BadRequest,
            "Regla de negocio incumplida",
            exception.Message),

        _ => (
            StatusCodes.Status500InternalServerError,
            "Error interno del servidor",
            "Ocurrió un error inesperado al procesar la solicitud.")
    };
}
