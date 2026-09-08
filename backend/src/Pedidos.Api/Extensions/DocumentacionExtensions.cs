using Pedidos.Api.OpenApi;

namespace Pedidos.Api.Extensions;

internal static class DocumentacionExtensions
{
    private const string RutaDelDocumento = "/openapi/v1.json";

    public static IServiceCollection AddDocumentacionOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(opciones => opciones.AddDocumentTransformer<TransformadorDeSeguridad>());

        return services;
    }

    public static void UseDocumentacionOpenApi(this WebApplication app)
    {
        app.MapOpenApi();

        app.UseSwaggerUI(opciones =>
        {
            opciones.SwaggerEndpoint(RutaDelDocumento, "API de Pedidos v1");
            opciones.RoutePrefix = "swagger";
            opciones.DocumentTitle = "API de Pedidos";
            opciones.DisplayRequestDuration();
        });
    }
}
