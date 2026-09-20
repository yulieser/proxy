using CRProxy.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

RegisterIoC.SetupIoCContainer(builder.Configuration, builder.Services);
RegisterIoC.SetupHostedService(builder.Services);

var app = builder.Build();

app.MapControllers();

// Expose minimal health route for liveness/readiness
app.MapGet("/healthz", (CRProxy.Infrastructure.Observability.ProxyMetrics metrics) =>
{
    return Results.Ok(new { status = "ok", activeConnections = metrics.ActiveConnections, errors = metrics.Errors });
});

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
