using EntitiesLibrary.Common;
using EntitiesLibrary.Services;
using GardenAPI.Data;

namespace GardenAPI.Service.Common
{
    public class PlantVarietyService(DataContext dataContext) : DataEntityService<PlantVariety>(dataContext)
    {
    }
}
