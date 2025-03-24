using AssistantAPI.Data;
using EntitiesLibrary.Events;
using EntitiesLibrary.Services;

namespace GardenAPI.Service.Common
{
    public class EventService(DataContext dataContext) : HasUserIdEntityService<Event>(dataContext)
    {
    }
}
