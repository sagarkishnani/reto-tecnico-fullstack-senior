using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pedidos.Infrastructure.Persistence;

internal sealed class PedidosDbContextFactory : IDesignTimeDbContextFactory<PedidosDbContext>
{
    private const string CadenaDeConexionLocal =
        "Host=localhost;Port=5432;Database=pedidos_db;Username=pedidos_user;Password=pedidos_pass";

    public PedidosDbContext CreateDbContext(string[] args)
    {
        var cadenaDeConexion =
            Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? CadenaDeConexionLocal;

        var opciones = new DbContextOptionsBuilder<PedidosDbContext>()
            .UseNpgsql(cadenaDeConexion)
            .Options;

        return new PedidosDbContext(opciones);
    }
}
