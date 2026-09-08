using Pedidos.Api.Errores;
using Pedidos.Api.Extensions;
using Pedidos.Application;
using Pedidos.Infrastructure;
using Pedidos.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.UseSerilogDePedidos();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorGlobalDeExcepciones>();
builder.Services.AddRateLimitingDePedidos(builder.Configuration);
builder.Services.AddCorsDelClienteWeb(builder.Configuration);
builder.Services.AddAutenticacionJwt(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var inicializador = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await inicializador.InicializarAsync();
}

app.UseRegistroDePeticiones();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(CorsExtensions.PoliticaDelCliente);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Iniciando Pedidos.Api.");
    app.Run();
}
catch (Exception excepcion)
{
    Log.Fatal(excepcion, "La aplicación terminó inesperadamente.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
