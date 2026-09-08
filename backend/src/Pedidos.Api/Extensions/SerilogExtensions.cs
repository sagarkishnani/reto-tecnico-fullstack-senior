using Serilog;
using Serilog.Events;

namespace Pedidos.Api.Extensions;

internal static class SerilogExtensions
{
    public static void UseSerilogDePedidos(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((contexto, servicios, configuracion) => configuracion
            .ReadFrom.Configuration(contexto.Configuration)
            .ReadFrom.Services(servicios)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Aplicacion", "Pedidos.Api"));
    }

    public static void UseRegistroDePeticiones(this WebApplication app)
    {
        app.UseSerilogRequestLogging(opciones =>
        {
            opciones.MessageTemplate =
                "{RequestMethod} {RequestPath} respondio {StatusCode} en {Elapsed:0.0} ms";

            opciones.GetLevel = (contexto, _, excepcion) =>
                excepcion is not null || contexto.Response.StatusCode >= StatusCodes.Status500InternalServerError
                    ? LogEventLevel.Error
                    : contexto.Response.StatusCode >= StatusCodes.Status400BadRequest
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;

            opciones.EnrichDiagnosticContext = (diagnostico, contexto) =>
            {
                diagnostico.Set("TraceId", contexto.TraceIdentifier);

                if (contexto.User.Identity?.IsAuthenticated == true)
                {
                    diagnostico.Set("UsuarioId", contexto.User.FindFirst("sub")?.Value);
                    diagnostico.Set("Rol", contexto.User.FindFirst("role")?.Value);
                }
            };
        });
    }
}
