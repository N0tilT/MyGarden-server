using AssistantAPI.Data;
using EntitiesLibrary.Data;
using EntitiesLibrary.Middleware;
using Prometheus;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

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

Log.Logger = new LoggerConfiguration()
    .WriteTo
    .GrafanaLoki("http://loki:3100")
    .CreateLogger();

application.UseMetricServer(url: "/metrics");
application.UseHttpMetrics();

application.Run();

void RegisterCoreServices(IServiceCollection services)
{
    //services.AddScoped<GrowStageServive>();
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
}
