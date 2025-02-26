using GardenAPI.Data;
using GardenAPI.Entities.Events;

namespace GardenAPI.Service.Common
{
    public class NotificationService(DataContext dataContext) : DataEntityService<Notification>(dataContext)
    {
    }
}
