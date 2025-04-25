using EntitiesLibrary.Plants;
using EntitiesLibrary.Transfer.Plant;
using GardenAPI.Controllers;
using GardenAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace PlantTest
{
    [TestFixture]
    internal class PlantControllerTests : CommonSetUp
    {
        internal required PlantController _plantController;

        [OneTimeSetUp]
        public void SetUp()
        {
            _plantController = new PlantController(_plantService);
        }

        [Test, Order(1)]
        public async Task Post_ReturnsOkResult_WhenPlantsSaved()
        {
            // Arrange
            var request = new List<RequestPlantDTO>
        {
            new RequestPlantDTO {
                LightNeedId = 1,
                StageId = 1,
                Title = "meow",
                BiologyTitle = "meow",
                UserId = "meow",
                WateringNeedId = 1,
                GroupId = 0,
                PlantTypeId = 1,
                PlantVarietyId = 1,
            },
            new RequestPlantDTO {
                LightNeedId = 1,
                StageId = 1,
                Title = "meow2",
                BiologyTitle = "meow2",
                UserId = "meow2",
                WateringNeedId = 1,
                GroupId = 0,
                PlantTypeId = 1,
                PlantVarietyId = 1,
            }
        };
            // Act
            var result = await _plantController!.Post(request);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }
        [Test, Order(2)]
        public async Task Get_ReturnsOkResult_WithPlants()
        {
            // Arrange
            var userId = "meow";
            var ids = new List<int> { 1, 2 };
            var entities = new List<Plant>
            {
                new Plant { UserId = "meow", GroupId = 0 },
            };

            var p = new List<PlantDTO>();
            // Act
            var result = await _plantController!.Get(userId, ids);
            if (result.Result is OkObjectResult okResult)
            {
                p = okResult.Value as List<PlantDTO>;
            }

            // ClassicAssert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsAssignableFrom<List<PlantDTO>>(p);
            ClassicAssert.AreEqual(entities.Count, p?.Count() ?? 0);
        }

        [Test, Order(3)]
        public async Task Post_ReturnsOkResult_WhenPlantsChanged()
        {
            // Arrange
            var request = new List<RequestPlantDTO>
        {
            new RequestPlantDTO {
                Id = 1,
                LightNeedId = 1,
                StageId = 1,
                Title = "meow",
                BiologyTitle = "meow",
                UserId = "meow",
                WateringNeedId = 1,
                GroupId = 0,
                PlantTypeId = 1,
                PlantVarietyId = 1,
            },
            new RequestPlantDTO {
                Id = 2,
                LightNeedId = 1,
                StageId = 1,
                Title = "meow3",
                BiologyTitle = "meow3",
                UserId = "meow2",
                WateringNeedId = 1,
                GroupId = 0,
                PlantTypeId = 1,
                PlantVarietyId = 1,
            }
        };

            // Act
            var result = await _plantController!.Post(request);

            // ClassicAssert
            var badRequestResult = result as OkObjectResult;
            ClassicAssert.IsInstanceOf<OkResult>(result);

        }

        [Test, Order(4)]
        public async Task Delete_ReturnsOkResult_WhenPlantsDeleted()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };

            // Act
            var result = await _plantController!.Delete(ids);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }

        [Test, Order(5)]
        public async Task Delete_ReturnsBadRequest_WhenPlantsNotDeleted()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };

            // Act
            var result = await _plantController!.Delete(ids);

            // ClassicAssert
            var badRequestResult = result as BadRequestObjectResult;
            ClassicAssert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult.");
            ClassicAssert.AreEqual("No plants were deleted!", badRequestResult!.Value);
        }
    }
}