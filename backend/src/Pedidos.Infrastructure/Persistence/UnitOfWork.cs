using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pedidos.Application.Abstractions.Persistence;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;
using Pedidos.Infrastructure.Resiliencia;

namespace Pedidos.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private const string CodigoViolacionDeUnicidad = "23505";

    private readonly PedidosDbContext _context;
    private readonly IEjecutorDeBaseDeDatos _ejecutor;

    public UnitOfWork(PedidosDbContext context, IEjecutorDeBaseDeDatos ejecutor)
    {
        _context = context;
        _ejecutor = ejecutor;
    }

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        _ejecutor.EscribirAsync(async token =>
        {
            try
            {
                return await _context.SaveChangesAsync(token);
            }
            catch (DbUpdateException excepcion) when (EsNumeroPedidoDuplicado(excepcion))
            {
                throw new NumeroPedidoDuplicadoException(ObtenerNumeroEnConflicto(excepcion));
            }
        }, cancellationToken);

    private static bool EsNumeroPedidoDuplicado(DbUpdateException excepcion) =>
        excepcion.InnerException is PostgresException postgres
        && postgres.SqlState == CodigoViolacionDeUnicidad
        && postgres.ConstraintName == PedidosDbContext.IndiceNumeroPedidoUnico;

    private static string ObtenerNumeroEnConflicto(DbUpdateException excepcion) =>
        excepcion.Entries
            .Select(entrada => entrada.Entity)
            .OfType<Pedido>()
            .Select(pedido => pedido.NumeroPedido)
            .FirstOrDefault() ?? string.Empty;
}
