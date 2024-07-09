using MyGarden.Server.Configuration.Repository;
using MyGarden.Server.Data;
using MyGarden.Server.Data.Initialization;
using MyGarden.Server.Middleware;
using MyGarden.Server.Service;
using MyGarden.Server.Service.Common;
using MyGarden.Server.Service.Plants;
using ConfigurationManager = MyGarden.Server.Configuration.ConfigurationManager;

#if DEBUG
 const bool IsDebugMode = true;
#else
    private const bool IsDebugMode = false;
#endif

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationManager(builder.Configuration);

RegisterCoreServices(builder.Services);
RegisterDataSources(builder.Services, configuration.DataConfiguration);
RegisterCorsServices(builder.Services);

var application = builder.Build();

application.UseMiddleware<ExceptionHandler>();
application.UseAuthentication();
application.UseAuthorization();
application.MapControllers();
application.UseCors();

InitializeDataSources(application);

application.Run();


/// <summary>
///     ���������������� �������� ������� � �����������.
/// </summary>
/// <param name="services">��������� ��������.</param>
void RegisterCoreServices(IServiceCollection services)
{
    services.AddScoped<DataEntityService>();
    services.AddScoped<GroupService>();
    services.AddScoped<PlantService>();
    services.AddScoped<EventService>();
    services.AddScoped<NotificationService>();
    services.AddScoped<GrowStageServive>();
    services.AddScoped<LightNeedService>();
    services.AddScoped<WateringNeedService>();

    services.AddTransient<ConfigurationManager>();
    services.AddControllers();
}

/// <summary>
///     ���������������� ��������� ������.
/// </summary>
/// <param name="services">��������� ��������.</param>
/// <param name="configuration">������������ ������.</param>
void RegisterDataSources(IServiceCollection services, DataConfiguration configuration)
{
    var dataConfiguration = configuration.GetDefaultContextConfiguration(IsDebugMode);
    services.AddScoped(provider => new DataContext(dataConfiguration));

    services.AddScoped<DataInitializationScript>();
}

/// <summary>
///     ���������������� ������� ����������� ��������������.
/// </summary>
/// <param name="services">��������� ��������.</param>
void RegisterCorsServices(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
    });
}

/// <summary>
///     ���������������� ������ ������.
/// </summary>
/// <param name="application">����������.</param>
async void InitializeDataSources(WebApplication application)
{
    using var scope = application.Services.CreateScope();

    await scope.ServiceProvider.GetRequiredService<DataInitializationScript>().Run();
}

