using Pedidos.Api.Errores;
using Pedidos.Api.Extensions;
using Pedidos.Application;
using Pedidos.Infrastructure;
using Pedidos.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorGlobalDeExcepciones>();
builder.Services.AddAutenticacionJwt(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var inicializador = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await inicializador.InicializarAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
