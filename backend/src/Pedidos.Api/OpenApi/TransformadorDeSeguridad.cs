using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Pedidos.Api.OpenApi;

internal sealed class TransformadorDeSeguridad : IOpenApiDocumentTransformer
{
    private const string EsquemaBearer = "Bearer";

    public Task TransformAsync(
        OpenApiDocument documento,
        OpenApiDocumentTransformerContext contexto,
        CancellationToken cancellationToken)
    {
        documento.Info = new OpenApiInfo
        {
            Title = "API de Pedidos",
            Version = "v1",
            Description =
                "API REST para la gestión de pedidos con autenticación JWT. " +
                "Use POST /auth/login para obtener un token y pulse Authorize para autenticarse."
        };

        documento.Components ??= new OpenApiComponents();
        documento.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        documento.Components.SecuritySchemes[EsquemaBearer] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Pegue únicamente el token, sin el prefijo 'Bearer'."
        };

        var requisito = new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = EsquemaBearer,
                    Type = ReferenceType.SecurityScheme
                }
            }] = []
        };

        foreach (var operacion in documento.Paths
            .Where(ruta => !ruta.Key.StartsWith("/auth", StringComparison.OrdinalIgnoreCase))
            .SelectMany(ruta => ruta.Value.Operations.Values))
        {
            operacion.Security = [requisito];
        }

        return Task.CompletedTask;
    }
}
