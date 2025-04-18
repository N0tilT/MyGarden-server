using EntitiesLibrary.Common;
using EntitiesLibrary.Services;
using GardenAPI.Data;

namespace GardenAPI.Service.Common
{
    public class PlantTypeService(DataContext dataContext) : DataEntityService<PlantType>(dataContext)
    {
    }
}
