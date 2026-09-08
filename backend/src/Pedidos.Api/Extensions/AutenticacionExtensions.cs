using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Pedidos.Infrastructure;
using Pedidos.Infrastructure.Security;

namespace Pedidos.Api.Extensions;

internal static class AutenticacionExtensions
{
    public static IServiceCollection AddAutenticacionJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var opciones = DependencyInjection.LeerOpcionesJwt(configuration);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(bearer =>
            {
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = opciones.Issuer,
                    ValidAudience = opciones.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opciones.Key)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = ClaimsDePedidos.Rol,
                    NameClaimType = ClaimsDePedidos.Nombre
                };
            });

        services.AddAuthorization();

        return services;
    }
}
