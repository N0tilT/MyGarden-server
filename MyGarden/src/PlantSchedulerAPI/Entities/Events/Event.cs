﻿using GardenAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenAPI.Entities.Events
{
    [Index(nameof(UserId))]
    public class Event : IdentifiableEntity, IHasUserId
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

        public const int TitleLengthMax = 256;
        public const bool IsUserIdRequired = true;
        public const bool IsTitleRequired = false;
        public const bool IsDateRequired = false;
        public const bool IsPlantIdRequired = true;

        /// <summary>
        ///     Конфигурация модели <see cref="Subject" />.
        /// </summary>
        /// <param name="configuration">Конфигурация базы данных.</param>
        internal class Configuration(BaseConfiguration configuration) : Configuration<Event>(configuration)
        {
            /// <summary>
            ///     Задать конфигурацию для модели.
            /// </summary>
            /// <param name="builder">Набор интерфейсов настройки модели.</param>
            public override void Configure(EntityTypeBuilder<Event> builder)
            {
                builder.Property(ev => ev.UserId)
                    .IsRequired(IsUserIdRequired);
                builder.HasOne(nutrition => nutrition.Plant)
                    .WithMany(plant => plant.Events)
                    .HasForeignKey(nutrition => nutrition.PlantId)
                    .IsRequired(IsPlantIdRequired);

                builder.Property(nutrition => nutrition.Title)
                   .HasMaxLength(TitleLengthMax)
                   .IsRequired(IsTitleRequired);

                builder.Property(nutrition => nutrition.Date)
                   .IsRequired(IsDateRequired);

                base.Configure(builder);
            }
        }

        #endregion

        /*             _   _ _
         *   ___ _ __ | |_(_) |_ _   _
         *  / _ \ '_ \| __| | __| | | |
         * |  __/ | | | |_| | |_| |_| |
         *  \___|_| |_|\__|_|\__|\__, |
         *                       |___/
         * Поля данных, соответствующие таковым в таблице
         * модели в базе данных.
         */

        #region Entity

        /// <summary>
        ///     Название уровня потребности растения в воде.
        /// </summary>
        public string? Title { get; set; }

        #endregion

        public required string UserId { get; set; }
        public required int PlantId { get; set; }

        public Plant? Plant { get; set; }
        public DateTime? Date { get; set; } = DateTime.MinValue;

        public List<Notification> Notifications { get; set; } = [];
    }
}
