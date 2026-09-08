using Pedidos.Domain.Entities;

namespace Pedidos.Application.Abstractions.Security;

public interface IGeneradorDeToken
{
    TokenGenerado Generar(Usuario usuario);
}

public sealed record TokenGenerado(string Token, int ExpiraEnSegundos);
