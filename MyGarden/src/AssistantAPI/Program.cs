using AssistantAPI.Data;
using AssistantAPI.Service.Gardens;
using EntitiesLibrary.Common;
using EntitiesLibrary.Data;
using EntitiesLibrary.Middleware;
using GardenAPI.Service.Common;
using Prometheus;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("app", Environment.GetEnvironmentVariable("APP_NAME"))
    .WriteTo.GrafanaLoki(
        "http://loki:3100",
        labels: new List<LokiLabel> { new LokiLabel { Key = "app", Value = Environment.GetEnvironmentVariable("APP_NAME") } }
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddEnvironmentVariables();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

RegisterCoreServices(builder.Services);
RegisterDataSources(builder.Services);

var application = builder.Build();

application.UseMiddleware<ExceptionHandler>();
application.UseSwagger();
application.UseSwaggerUI();
application.MapControllers();
application.MapHealthChecks("/health");
application.UseCors();

await InitializeDataSources(application);

application.UseMetricServer(url: "/metrics");
application.UseHttpMetrics();

application.Run();

void RegisterCoreServices(IServiceCollection services)
{
    services.AddScoped<EventService>();
    services.AddScoped<GardenTypeService>();
    services.AddScoped<GardenService>();
    services.AddControllers();
}

void RegisterDataSources(IServiceCollection services)
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
    var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
    var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    var connectionString = $"Server={dbHost};Port=5432;Database={dbName};User Id={dbUser};Password={dbPassword};";
    services.AddScoped(provider => new DataContext(new ContextConfiguration(connectionString,"assistantAPI")));
}

async Task InitializeDataSources(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    await dataContext.TryInitializeAsync();

    await scope.ServiceProvider.GetRequiredService<GardenTypeService>().Set(dataContext.GardenTypes, new List<GardenType>{
                new GardenType{Id = 1,Title="Сад"},
                new GardenType{Id = 2,Title="Полка" },
            });
}

