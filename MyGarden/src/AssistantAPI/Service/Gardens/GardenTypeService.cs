using AssistantAPI.Data;
using EntitiesLibrary.Common;
using EntitiesLibrary.Services;

namespace AssistantAPI.Service.Gardens
{
    public class GardenTypeService(DataContext dataContext) : DataEntityService<GardenType>(dataContext)
    {
    }
}
