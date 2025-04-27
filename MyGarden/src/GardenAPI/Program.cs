using EntitiesLibrary.Common;
using EntitiesLibrary.Data;
using EntitiesLibrary.Middleware;
using GardenAPI.Data;
using GardenAPI.Service.Common;
using GardenAPI.Service.Plants;
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
    services.AddScoped<GrowStageService>();
    services.AddScoped<LightNeedService>();
    services.AddScoped<WateringNeedService>();
    services.AddScoped<GroupService>();
    services.AddScoped<PlantService>();
    services.AddScoped<PlantTypeService>();
    services.AddScoped<PlantVarietyService>();
    services.AddControllers();
}

void RegisterDataSources(IServiceCollection services)
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
    var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
    var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    var connectionString = $"Server={dbHost};Port=5432;Database={dbName};User Id={dbUser};Password={dbPassword};";
    services.AddScoped(provider => new DataContext(new ContextConfiguration(connectionString,"gardenAPI")));
}

async Task InitializeDataSources(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    await dataContext.TryInitializeAsync();

    await scope.ServiceProvider.GetRequiredService<GrowStageService>().Set(dataContext.GrowStages, new List<GrowStage>{
                new GrowStage{Id = 1,Title="Нет"},
                new GrowStage{Id = 2,Title="Семя" },
                new GrowStage{Id = 3,Title="Росток"},
                new GrowStage{Id = 4,Title="Молодой"},
                new GrowStage{Id = 5,Title="Плодоносящий"},
                new GrowStage{Id = 6,Title="Состарившееся"}
            });

    await scope.ServiceProvider.GetRequiredService<LightNeedService>().Set(dataContext.LightNeeds, new List<LightNeed> {
                new LightNeed{Id=1,Title="Нет"},
                new LightNeed{Id=2,Title="Низкая"},
                new LightNeed{Id=3,Title="Средняя"},
                new LightNeed{Id=4,Title="Высокая"}
    });

    await scope.ServiceProvider.GetRequiredService<WateringNeedService>().Set(dataContext.WateringNeeds, new List<WateringNeed> {
                new WateringNeed{Id=1,Title="Нет"},
                new WateringNeed{Id=2,Title="Низкая"},
                new WateringNeed{Id=3,Title="Средняя"},
                new WateringNeed{Id=4,Title="Высокая"}
            });

    await scope.ServiceProvider.GetRequiredService<PlantTypeService>().Set(dataContext.PlantTypes, new List<PlantType> {
                new PlantType{Id=1,Title="Без типа"},
                new PlantType{Id=2,Title="Цветок"},
                new PlantType{Id=3,Title="Дерево"},
                new PlantType{Id=4,Title="Овощ"},
            });
    await scope.ServiceProvider.GetRequiredService<PlantVarietyService>().Set(dataContext.PlantVarieties, new List<PlantVariety> {
                new PlantVariety{Id=1,Title="Без вида",PlantTypeId=1},
                new PlantVariety{Id=2,Title="Роза",PlantTypeId=2},
                new PlantVariety{Id=3,Title="Огурец",PlantTypeId=3}
            });
}
