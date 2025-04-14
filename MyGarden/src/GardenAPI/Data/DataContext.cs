using EntitiesLibrary.Common;
using EntitiesLibrary.Data;
using EntitiesLibrary.Events;
using EntitiesLibrary.Gardens;
using EntitiesLibrary.Plants;
using Microsoft.EntityFrameworkCore;

namespace GardenAPI.Data
{
    public class DataContext(BaseConfiguration configuration) : DbContext
    {
        private BaseConfiguration Configuration { get; } = configuration;

        /// <summary>
        ///     Обработать настройку сессии.
        /// </summary>
        /// <param name="optionsBuilder">Набор интерфейсов настройки сессии.</param>
        /// <exception cref="Exception">При ошибке подключения.</exception>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Configuration.ConfigureContext(optionsBuilder);

            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        ///     Попытаться асинхронно инициализировать сессию.
        ///     Используется для проверки подключения
        ///     и инициализации структуры таблиц.
        /// </summary>
        /// <returns>Статус успешности инициализации.</returns>
        public async Task<bool> TryInitializeAsync()
        {
            try
            {
                await Database.MigrateAsync();
                return await Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Обработать инициализацию модели.
        ///     Используется для дополнительной настройки модели.
        /// </summary>
        /// <param name="modelBuilder">Набор интерфейсов настройки модели.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new Group.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new Plant.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new Event.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new Notification.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new GrowStage.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new LightNeed.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new WateringNeed.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new Garden.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new GardenType.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new PlantType.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new PlantVariety.Configuration(Configuration));

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Plant> Plants => Set<Plant>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<GrowStage> GrowStages => Set<GrowStage>();
        public DbSet<LightNeed> LightNeeds => Set<LightNeed>();
        public DbSet<WateringNeed> WateringNeeds => Set<WateringNeed>();
        public DbSet<Garden> Gardens => Set<Garden>();
        public DbSet<GardenType> GardenTypes => Set<GardenType>();
        public DbSet<PlantType> PlantTypes => Set<PlantType>();
        public DbSet<PlantVariety> PlantVarieties => Set<PlantVariety>();
    }
}




