using EntitiesLibrary.Gardens;
using EntitiesLibrary.Services;
using GardenAPI.Data;

namespace GardenAPI.Service.Gardens
{
    public class GardenService(DataContext dataContext) : HasUserIdEntityService<Garden>(dataContext)
    {
    }
}
