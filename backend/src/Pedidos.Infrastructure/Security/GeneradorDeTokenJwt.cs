using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pedidos.Application.Abstractions.Security;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Security;

internal sealed class GeneradorDeTokenJwt : IGeneradorDeToken
{
    private readonly JwtOptions _opciones;

    public GeneradorDeTokenJwt(IOptions<JwtOptions> opciones)
    {
        _opciones = opciones.Value;
    }

    public TokenGenerado Generar(Usuario usuario)
    {
        var emitidoEn = DateTime.UtcNow;
        var expiraEn = emitidoEn.AddMinutes(_opciones.ExpiracionEnMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimsDePedidos.Nombre, usuario.Nombre),
            new(ClaimsDePedidos.Rol, usuario.Rol.ToString())
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: emitidoEn,
            expires: expiraEn,
            signingCredentials: credenciales);

        return new TokenGenerado(
            new JwtSecurityTokenHandler().WriteToken(token),
            (int)(expiraEn - emitidoEn).TotalSeconds);
    }
}
