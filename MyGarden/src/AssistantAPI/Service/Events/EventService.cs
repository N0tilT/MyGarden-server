using AssistantAPI.Data;
using AssistantAPI.Entities.Events;
using AssistantAPI.Service;

namespace GardenAPI.Service.Common
{
    public class EventService(DataContext dataContext) : HasUserIdEntityService<Event>(dataContext)
    {
    }
}
