using EntitiesLibrary.Common;
using EntitiesLibrary.Data;
using EntitiesLibrary.Plants;
using GardenAPI.Data;
using GardenAPI.Service.Common;
using GardenAPI.Service.Gardens;
using GardenAPI.Service.Plants;
using NUnit.Framework;

namespace GardenAPI.Tests
{
    [TestFixture]
    internal class CommonSetUp
    {
        internal required DataContext _dataContext;
        internal required PlantTypeService _plantTypeService;
        internal required PlantVarietyService _plantVarietyService;
        internal required GrowStageService _growStageService;
        internal required LightNeedService _lightNeedService;
        internal required WateringNeedService _wateringNeedService;
        internal required GroupService _groupService;
        internal required GardenService _gardenService;
        internal required GardenTypeService _gardenTypeService;
        internal required PlantService _plantService;

        [OneTimeSetUp]
        public void Setup()
        {
            _dataContext = CreateInMemoryDbContext();

            _plantTypeService = new PlantTypeService(_dataContext);
            _plantVarietyService = new PlantVarietyService(_dataContext);
            _growStageService = new GrowStageService(_dataContext);
            _lightNeedService = new LightNeedService(_dataContext);
            _wateringNeedService = new WateringNeedService(_dataContext);
            _groupService = new GroupService(_dataContext);
            _gardenService = new GardenService(_dataContext);
            _gardenTypeService = new GardenTypeService(_dataContext);
            _plantService = new PlantService(_dataContext);

            Task.WhenAll(
                _growStageService.Set(_dataContext.GrowStages,
                [
                    new GrowStage{Id = 1,Title="Семя"},
                    new GrowStage{Id = 2,Title="Прорастание"},
                    new GrowStage{Id = 3,Title="Рост"},
                    new GrowStage{Id = 4,Title="Цветение"},
                    new GrowStage{Id = 5,Title="Плодоносение"},
                    new GrowStage{Id = 6,Title="Упадок"}
                ]),
                _lightNeedService.Set(_dataContext.LightNeeds,
                [
                    new LightNeed{Id=1,Title="Низкий"},
                    new LightNeed{Id=2,Title="Средний"},
                    new LightNeed{Id=3,Title="Высокий"}
                ]),
                _wateringNeedService.Set(_dataContext.WateringNeeds,
                [
                    new WateringNeed{Id=1,Title="Низкий"},
                    new WateringNeed{Id=2,Title="Средний"},
                    new WateringNeed{Id=3,Title="Высокий"}
                ]),
                _plantTypeService.Set(_dataContext.PlantTypes,
                [
                    new PlantType{Id=1,Title="Base"}
                ]),
                _plantVarietyService.Set(_dataContext.PlantVarieties,
                [
                    new PlantVariety{Id=1,Title="Base",PlantTypeId=1}
                ]),
            _groupService.Set(_dataContext.Groups,
            [
                    new Group{Id=0,UserId = "default",Title="default"},
            ])
            ).GetAwaiter().GetResult();

        }

        private DataContext CreateInMemoryDbContext()
        {
            MockConfiguration configuration = new MockConfiguration();
            return new DataContext(configuration);
        }
    }
}
