using EntitiesLibrary.Plants;
using EntitiesLibrary.Services;
using GardenAPI.Data;

namespace GardenAPI.Service.Plants
{
    public class PlantService(DataContext dataContext) : HasUserIdEntityService<Plant>(dataContext)
    {
    }
}
