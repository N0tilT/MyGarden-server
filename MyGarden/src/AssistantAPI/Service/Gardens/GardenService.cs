using AssistantAPI.Data;
using EntitiesLibrary.Gardens;
using EntitiesLibrary.Services;

namespace AssistantAPI.Service.Gardens
{
    public class GardenService(DataContext dataContext) : HasUserIdEntityService<Garden>(dataContext)
    {
    }
}
