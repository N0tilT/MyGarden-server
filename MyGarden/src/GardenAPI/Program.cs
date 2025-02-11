using GardenAPI.Data;
using GardenAPI.Entities.Common;
using GardenAPI.Entities.Plants;
using GardenAPI.Middleware;
using GardenAPI.Service.Common;
using GardenAPI.Service.Plants;
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
    services.AddScoped<GrowStageServive>();
    services.AddScoped<LightNeedService>();
    services.AddScoped<WateringNeedService>();
    services.AddScoped<EventService>();
    services.AddScoped<GroupService>();
    services.AddScoped<PlantService>();
    services.AddControllers();
}

void RegisterDataSources(IServiceCollection services)
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
    var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
    var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    var connectionString = $"Server={dbHost};Port=5432;Database={dbName};User Id={dbUser};Password={dbPassword};";
    services.AddScoped(provider => new DataContext(new ContextConfiguration(connectionString)));
}

async Task InitializeDataSources(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    await dataContext.TryInitializeAsync();

    await scope.ServiceProvider.GetRequiredService<GrowStageServive>().Set(dataContext.GrowStages, new List<GrowStage>{
                new GrowStage{Id = 1,Title="����"},
                new GrowStage{Id = 2,Title="�����������" },
                new GrowStage{Id = 3,Title="����"},
                new GrowStage{Id = 4,Title="��������"},
                new GrowStage{Id = 5,Title="������������"},
                new GrowStage{Id = 6,Title="������"}
            });

    await scope.ServiceProvider.GetRequiredService<LightNeedService>().Set(dataContext.LightNeeds, new List<LightNeed> {
                new LightNeed{Id=1,Title="������"},
                new LightNeed{Id=2,Title="�������"},
                new LightNeed{Id=3,Title="�������"}
    });

    await scope.ServiceProvider.GetRequiredService<WateringNeedService>().Set(dataContext.WateringNeeds, new List<WateringNeed> {
                new WateringNeed{Id=1,Title="������"},
                new WateringNeed{Id=2,Title="�������"},
                new WateringNeed{Id=3,Title="�������"}
            });

    await scope.ServiceProvider.GetRequiredService<GroupService>().Set(dataContext.Groups, new List<Group>
    {
        new Group{Id=0,UserId = "default",Title="default"},
    });


}
