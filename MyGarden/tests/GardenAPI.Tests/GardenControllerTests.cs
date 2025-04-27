
using EntitiesLibrary.Gardens;
using EntitiesLibrary.Transfer.Garden;
using GardenAPI.Controllers;
using GardenAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace GardenTest
{
    [TestFixture]
    internal class GardenControllerTests : CommonSetUp
    {
        internal required GardenController _gardenController;

        [OneTimeSetUp]
        public void SetUp()
        {
            _gardenController = new GardenController(_gardenService);
        }

        [Test, Order(1)]
        public async Task Post_ReturnsOkResult_WhenGardensSaved()
        {
            // Arrange
            var gardenRequest = new List<RequestFlowerBedDTO>
            {
                new RequestFlowerBedDTO {
                    Beds = new List<Bed>(){ new Bed()
                    {
                        Id = 0,
                        X = 0,
                        Y = 0,
                        Width = 0,
                        Height = 0,
                        RotationAngle = 0,
                        AdditionalInfo = "string",
                        Plants = [1,2,3,4,5]
                    } },
                    UserId = "meow",
                },
                new RequestFlowerBedDTO {
                    Beds = new List<Bed>(){ new Bed()
                    {
                        Id = 3,
                        X = 0,
                        Y = 0,
                        Width = 0,
                        Height = 0,
                        RotationAngle = 0,
                        AdditionalInfo = "string",
                        Plants = [1,2,3,4,5]
                    } },
                    UserId = "meow2",
                },
            };
            // Act
            var result = await _gardenController!.Post(gardenRequest);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }
        [Test, Order(2)]
        public async Task Get_ReturnsOkResult_WithGardens()
        {
            // Arrange
            var userId = "meow";
            var ids = new List<int> { 1, 2 };
            var entities = new List<Garden>
            {
                new Garden { UserId = "meow", },
            };

            var p = new List<FlowerBedDTO>();
            // Act
            var result = await _gardenController!.Get(userId, ids);
            if (result.Result is OkObjectResult okResult)
            {
                p = okResult.Value as List<FlowerBedDTO>;
            }

            // ClassicAssert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsAssignableFrom<List<FlowerBedDTO>>(p);
            ClassicAssert.AreEqual(entities.Count, p?.Count() ?? 0);
        }

        [Test, Order(3)]
        public async Task Post_ReturnsOkResult_WhenGardensChanged()
        {
            // Arrange
            var request = new List<RequestFlowerBedDTO>
        {
            new RequestFlowerBedDTO {
                Id = 1,
                Beds = new List<Bed>(){ new Bed()
                    {
                        Id = 3,
                        X = 0,
                        Y = 0,
                        Width = 0,
                        Height = 0,
                        RotationAngle = 0,
                        AdditionalInfo = "string_changed",
                        Plants = [1,2,3,4,5]
                    } },
                UserId = "meow"
            },
            new RequestFlowerBedDTO {
                Id = 2,
                Beds = new List<Bed>(){ new Bed()
                    {
                        Id = 3,
                        X = 0,
                        Y = 0,
                        Width = 0,
                        Height = 0,
                        RotationAngle = 0,
                        AdditionalInfo = "string_changed",
                        Plants = [1,2,4,5]
                    } },
                UserId = "meow2"
            }
        };

            // Act
            var result = await _gardenController!.Post(request);

            // ClassicAssert
            var badRequestResult = result as OkObjectResult;
            ClassicAssert.IsInstanceOf<OkResult>(result);

        }

        [Test, Order(4)]
        public async Task Delete_ReturnsOkResult_WhenGardensDeleted()
        {
            // Arrange
            var gardenIds = new List<int> { 1, 2 };

            // Act
            var result = await _gardenController!.Delete(gardenIds);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }

        [Test, Order(5)]
        public async Task Delete_ReturnsBadRequest_WhenGardensNotDeleted()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };

            // Act
            var result = await _gardenController!.Delete(ids);

            // ClassicAssert
            var badRequestResult = result as BadRequestObjectResult;
            ClassicAssert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult.");
            ClassicAssert.AreEqual("No gardens were deleted!", badRequestResult!.Value);
        }
    }
}