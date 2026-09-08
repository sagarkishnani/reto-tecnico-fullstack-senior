namespace Pedidos.Domain.Common;

public abstract class EntidadBase
{
    public int Id { get; protected set; }

    public DateTime FechaCreacion { get; protected set; }

    public DateTime? FechaActualizacion { get; protected set; }

    public bool Eliminado { get; protected set; }

    public DateTime? FechaEliminacion { get; protected set; }

    public void Eliminar()
    {
        if (Eliminado)
        {
            return;
        }

        Eliminado = true;
        FechaEliminacion = DateTime.UtcNow;
    }

    protected void RegistrarCreacion() => FechaCreacion = DateTime.UtcNow;

    protected void RegistrarActualizacion() => FechaActualizacion = DateTime.UtcNow;
}
