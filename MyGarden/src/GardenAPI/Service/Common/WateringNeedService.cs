using EntitiesLibrary.Common;
using EntitiesLibrary.Services;
using GardenAPI.Data;

namespace GardenAPI.Service.Common
{
    public class WateringNeedService(DataContext dataContext) : DataEntityService<WateringNeed>(dataContext)
    {
    }
}
