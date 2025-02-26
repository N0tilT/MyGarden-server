using EntitiesLibrary.Common;
using EntitiesLibrary.Services;
using GardenAPI.Data;

namespace GardenAPI.Service.Common
{
    public class GardenTypeService(DataContext dataContext) : DataEntityService<GardenType>(dataContext)
    {
    }
}
