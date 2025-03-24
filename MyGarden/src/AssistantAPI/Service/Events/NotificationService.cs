using AssistantAPI.Data;
using EntitiesLibrary.Events;
using EntitiesLibrary.Services;

namespace GardenAPI.Service.Common
{
    public class NotificationService(DataContext dataContext) : DataEntityService<Notification>(dataContext)
    {
    }
}
