using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;
using Pedidos.Domain.Tests.Builders;

namespace Pedidos.Domain.Tests.Entities;

public class UsuarioTests
{
    [Fact]
    public void Crear_ConDatosValidos_DevuelveUsuarioConSusDatos()
    {
        var usuario = new UsuarioBuilder().ConRol(RolUsuario.Admin).Construir();

        Assert.Equal("user@email.com", usuario.Email);
        Assert.Equal("Juan Perez", usuario.Nombre);
        Assert.Equal(RolUsuario.Admin, usuario.Rol);
        Assert.False(usuario.Eliminado);
        Assert.NotEqual(default, usuario.FechaCreacion);
    }

    [Theory]
    [InlineData("USER@EMAIL.COM", "user@email.com")]
    [InlineData("  User@Email.Com  ", "user@email.com")]
    public void Crear_NormalizaElEmail(string entrada, string esperado)
    {
        var usuario = new UsuarioBuilder().ConEmail(entrada).Construir();

        Assert.Equal(esperado, usuario.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_ConEmailVacio_LanzaReglaDeNegocio(string? email)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new UsuarioBuilder().ConEmail(email!).Construir());
    }

    [Fact]
    public void Crear_ConEmailQueExcedeLaLongitudMaxima_LanzaReglaDeNegocio()
    {
        var emailLargo = new string('a', Usuario.LongitudMaximaEmail + 1);

        Assert.Throws<ReglaDeNegocioException>(
            () => new UsuarioBuilder().ConEmail(emailLargo).Construir());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_ConNombreVacio_LanzaReglaDeNegocio(string? nombre)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new UsuarioBuilder().ConNombre(nombre!).Construir());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_SinHashDePassword_LanzaReglaDeNegocio(string? passwordHash)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new UsuarioBuilder().ConPasswordHash(passwordHash!).Construir());
    }

    [Fact]
    public void CambiarPassword_ReemplazaElHashYRegistraLaFecha()
    {
        var usuario = new UsuarioBuilder().Construir();

        usuario.CambiarPassword("$2a$11$otroHash");

        Assert.Equal("$2a$11$otroHash", usuario.PasswordHash);
        Assert.NotNull(usuario.FechaActualizacion);
    }

    [Fact]
    public void CambiarPassword_ConHashVacio_LanzaReglaDeNegocio()
    {
        var usuario = new UsuarioBuilder().Construir();

        Assert.Throws<ReglaDeNegocioException>(() => usuario.CambiarPassword("  "));
    }
}
