﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyGarden.Server.Data.Configuration;
using MyGarden.Server.Entity.Events;

namespace MyGarden.Server.Entities.Events
{
    public class Notification : IdentifiableEntity
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

        public const bool IsEventIdRequired = true;

        /// <summary>
        ///     Конфигурация модели <see cref="Subject" />.
        /// </summary>
        /// <param name="configuration">Конфигурация базы данных.</param>
        internal class Configuration(ContextConfiguration configuration) : Configuration<Notification>(configuration)
        {
            /// <summary>
            ///     Задать конфигурацию для модели.
            /// </summary>
            /// <param name="builder">Набор интерфейсов настройки модели.</param>
            public override void Configure(EntityTypeBuilder<Notification> builder)
            {
                builder.HasOne(nutrition => nutrition.Event)
                    .WithMany(plant => plant.Notifications)
                    .HasForeignKey(nutrition => nutrition.EventId)
                    .IsRequired(IsEventIdRequired);

                base.Configure(builder);
            }
        }

        #endregion


        public required int EventId { get; set; }

        public Event? Event { get; set; }
    }
}