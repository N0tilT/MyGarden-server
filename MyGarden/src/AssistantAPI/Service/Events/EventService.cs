using AssistantAPI.Data;
using EntitiesLibrary.Events;
using EntitiesLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace GardenAPI.Service.Common
{
    public class EventService(DataContext dataContext) : HasUserIdEntityService<Event>(dataContext)
    {
        /// <summary>
        ///     Получить события по списку идентификаторов с учетом id пользователя и id растения.
        /// </summary>
        /// <param name="dbSet">Набор объектов <see cref="DbSet{TEntity}" />.</param>
        /// <param name="ids">Список идентификаторов.</param>
        /// <typeparam name="TSource">Тип модели.</typeparam>
        /// <returns>Список моделей.</returns>
        public async Task<List<Event>> Get(DbSet<Event> dbSet, string userId, List<int>? ids = null, List<int>? plantIds = null)
        {
            ids ??= [];
            plantIds ??= [];
            if (ids.Count <= 0)
            {
                if (plantIds.Count <= 0)
                {
                    return await dbSet.Where(entity => entity.UserId == userId || entity.Id == 0).ToListAsync();
                }
                else
                {
                    return await dbSet.Where(entity => (entity.UserId == userId && plantIds.Contains(entity.PlantId)) || entity.Id == 0).ToListAsync();
                }
            }
            else
            {
                if (plantIds.Count <= 0)
                {
                return await dbSet.Where(entity => entity.Id == 0 || (entity.UserId == userId && ids.Contains(entity.Id.GetValueOrDefault()))).ToListAsync();
                }
                else
                {
                    return await dbSet.Where(entity => entity.Id == 0 || (entity.UserId == userId && plantIds.Contains(entity.PlantId) && ids.Contains(entity.Id.GetValueOrDefault()))).ToListAsync();

                }
            }
        }
    }
}
