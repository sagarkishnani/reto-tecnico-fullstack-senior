namespace Pedidos.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SeccionDeConfiguracion = "Jwt";

    public const int LongitudMinimaDeClaveEnBytes = 32;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public int ExpiracionEnMinutos { get; set; } = 60;
}
