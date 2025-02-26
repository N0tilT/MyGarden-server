using EntitiesLibrary.Plants;
using EntitiesLibrary.Services;
using GardenAPI.Data;

namespace GardenAPI.Service.Plants
{
    public class GroupService(DataContext dataContext) : HasUserIdEntityService<Group>(dataContext)
    {
    }
}
