using EntitiesLibrary;
using EntitiesLibrary.Common;
using EntitiesLibrary.Data;
using EntitiesLibrary.Events;
using EntitiesLibrary.Gardens;
using EntitiesLibrary.Plants;
using Microsoft.EntityFrameworkCore;

namespace AssistantAPI.Data
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
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
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
            modelBuilder.ApplyConfiguration(new Event.Configuration(Configuration));
            modelBuilder.Entity<GrowStage>().ToTable("GrowStage", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<LightNeed>().ToTable("LightNeed", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<PlantType>().ToTable("PlantType", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<User>().ToTable("User", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<WateringNeed>().ToTable("WateringNeed", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<PlantVariety>().ToTable("PlantVariety", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Group>().ToTable("Group", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Plant>().ToTable("Plant", t => t.ExcludeFromMigrations());

            modelBuilder.ApplyConfiguration(new Garden.Configuration(Configuration));
            modelBuilder.ApplyConfiguration(new GardenType.Configuration(Configuration));

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Event> Events => Set<Event>();
        public DbSet<Garden> Gardens => Set<Garden>();
        public DbSet<GardenType> GardenTypes => Set<GardenType>();
    }
}




