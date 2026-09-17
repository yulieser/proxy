using CRProxy.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

RegisterIoC.SetupIoCContainer(builder.Configuration, builder.Services);
RegisterIoC.SetupHostedService(builder.Services);

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();
