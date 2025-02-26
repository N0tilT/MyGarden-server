using EntitiesLibrary.Common;
using EntitiesLibrary.Services;
using GardenAPI.Data;

namespace GardenAPI.Service.Common
{
    public class LightNeedService(DataContext dataContext) : DataEntityService<LightNeed>(dataContext)
    {
    }
}
