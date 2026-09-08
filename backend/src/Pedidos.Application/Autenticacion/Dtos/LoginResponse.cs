namespace Pedidos.Application.Autenticacion.Dtos;

public sealed record LoginResponse(string Token, int ExpiresIn);
