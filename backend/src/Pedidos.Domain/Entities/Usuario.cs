using Pedidos.Domain.Common;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Domain.Entities;

public sealed class Usuario : EntidadBase
{
    public const int LongitudMaximaEmail = 256;
    public const int LongitudMaximaNombre = 150;

    private Usuario()
    {
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public RolUsuario Rol { get; private set; }

    public static Usuario Crear(string email, string passwordHash, string nombre, RolUsuario rol)
    {
        var usuario = new Usuario
        {
            Email = NormalizarEmail(email),
            PasswordHash = ValidarPasswordHash(passwordHash),
            Nombre = NormalizarNombre(nombre),
            Rol = rol
        };

        usuario.RegistrarCreacion();
        return usuario;
    }

    public void CambiarPassword(string passwordHash)
    {
        PasswordHash = ValidarPasswordHash(passwordHash);
        RegistrarActualizacion();
    }

    private static string NormalizarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ReglaDeNegocioException("El email es obligatorio.");
        }

        var normalizado = email.Trim().ToLowerInvariant();

        if (normalizado.Length > LongitudMaximaEmail)
        {
            throw new ReglaDeNegocioException(
                $"El email no puede superar los {LongitudMaximaEmail} caracteres.");
        }

        return normalizado;
    }

    private static string NormalizarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ReglaDeNegocioException("El nombre es obligatorio.");
        }

        var normalizado = nombre.Trim();

        if (normalizado.Length > LongitudMaximaNombre)
        {
            throw new ReglaDeNegocioException(
                $"El nombre no puede superar los {LongitudMaximaNombre} caracteres.");
        }

        return normalizado;
    }

    private static string ValidarPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ReglaDeNegocioException("El hash de la contraseña es obligatorio.");
        }

        return passwordHash;
    }
}
