using EntitiesLibrary.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace EntitiesLibrary.Gardens
{
    [Index(nameof(UserId))]
    public class Garden : IdentifiableEntity, IHasUserId
    {
        /*                   __ _                       _   _
        *   ___ ___  _ __  / _(_) __ _ _   _ _ __ __ _| |_(_) ___  _ __
        *  / __/ _ \| '_ \| |_| |/ _` | | | | '__/ _` | __| |/ _ \| '_ \
        * | (_| (_) | | | |  _| | (_| | |_| | | | (_| | |_| | (_) | | | |
        *  \___\___/|_| |_|_| |_|\__, |\__,_|_|  \__,_|\__|_|\___/|_| |_|
        *                        |___/
        * Константы, задающие базовые конфигурации полей
        * и ограничения модели.
        */

        #region Configuration


        public const bool IsUserIdRequired = true;

        /// <summary>
        ///     Конфигурация модели <see cref="Garden" />.
        /// </summary>
        /// <param name="configuration">Конфигурация базы данных.</param>
        public class Configuration(BaseConfiguration configuration) : Configuration<Garden>(configuration)
        {
            /// <summary>
            ///     Задать конфигурацию для модели.
            /// </summary>
            /// <param name="builder">Набор интерфейсов настройки модели.</param>
            public override void Configure(EntityTypeBuilder<Garden> builder)
            {
                builder.Property(garden => garden.UserId)
                    .IsRequired(IsUserIdRequired);
                builder.Property(garden => garden.Beds)
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<Bed>>(v, (JsonSerializerOptions)null)
                    )
                    .Metadata.SetValueComparer(
                new ValueComparer<List<Bed>>(
                    (c1, c2) => c1.SequenceEqual(c2), // Compare element sequences
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Generate hash code
                    c => c.ToList() // Create a deep copy for snapshotting
                )
            ); ;
                base.Configure(builder);
            }
        }

        #endregion


        public required string UserId { get; set; }
        public List<Bed> Beds { get; set; } = [];
    }
}
