using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;

namespace Pedidos.Domain.Tests.Builders;

internal sealed class UsuarioBuilder
{
    private string _email = "user@email.com";
    private string _passwordHash = "$2a$11$hashDePrueba";
    private string _nombre = "Juan Perez";
    private RolUsuario _rol = RolUsuario.User;

    public UsuarioBuilder ConEmail(string email)
    {
        _email = email;
        return this;
    }

    public UsuarioBuilder ConPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public UsuarioBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    public UsuarioBuilder ConRol(RolUsuario rol)
    {
        _rol = rol;
        return this;
    }

    public Usuario Construir() => Usuario.Crear(_email, _passwordHash, _nombre, _rol);
}
