using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AssistantAPI.Data
{

    /// <summary>
    ///     Конфигурация контекста для работы с базой данных.
    /// </summary>
    /// <param name="connectionString">Строка подключения к базе данных.</param>
    /// <param name="isDebugMode">Статус конфигурации для разработки.</param>
    public class ContextConfiguration(string connectionString) : BaseConfiguration
    {
        public string ConnectionString { get; } = connectionString;
        private bool IsDebugMode { get; } = false;

        /// <summary>
        ///     Тип полей даты и времени в базе данных.
        /// </summary>
        internal override string DateTimeType => "timestamp with time zone";

        /// <summary>
        ///     Указатель использования текущих даты и времени
        ///     для полей типа <see cref="DateTimeType" /> в базе данных.
        /// </summary>
        internal override string DateTimeValueCurrent => "current_timestamp";

        /// <summary>
        ///     Применить настройки к сессии.
        /// </summary>
        /// <param name="optionsBuilder">Набор интерфейсов настройки сессии.</param>
        public override void ConfigureContext(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);

            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.ConfigureWarnings(builder => builder.Throw(RelationalEventId.MultipleCollectionIncludeWarning));

            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }

    }
}
