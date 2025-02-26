using EntitiesLibrary.Common;
using EntitiesLibrary.Services;
using GardenAPI.Data;

namespace GardenAPI.Service.Common
{
    public class GrowStageServive(DataContext dataContext) : DataEntityService<GrowStage>(dataContext)
    {
    }
}
