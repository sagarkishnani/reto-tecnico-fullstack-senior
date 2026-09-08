namespace Pedidos.Domain.Exceptions;

public sealed class RecursoNoEncontradoException : DomainException
{
    public RecursoNoEncontradoException(string recurso, object identificador)
        : base($"No se encontró {recurso} con identificador {identificador}.")
    {
        Recurso = recurso;
        Identificador = identificador;
    }

    public string Recurso { get; }

    public object Identificador { get; }
}
