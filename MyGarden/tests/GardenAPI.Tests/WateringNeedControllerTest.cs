using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using GardenAPI.Controllers.Common;
using GardenAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace WateringNeedTest
{
    [TestFixture]
    internal class WateringNeedControllerTests : CommonSetUp
    {
        internal required WateringNeedController _wateringNeedController;

        [OneTimeSetUp]
        public void SetUp()
        {
            _wateringNeedController = new WateringNeedController(_wateringNeedService);
        }

        [Test, Order(1)]
        public async Task Post_ReturnsOkResult_WhenwateringNeedsSaved()
        {
            // Arrange
            var wateringNeedRequest = new List<RequestCommonDTO>
        {
            new RequestCommonDTO {
                Title = "meow"
            },
            new RequestCommonDTO {
                Title = "meow2"
            }
        };
            // Act
            var result = await _wateringNeedController!.Post(wateringNeedRequest);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }
        [Test, Order(2)]
        public async Task Get_ReturnsOkResult_WithwateringNeeds()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };
            var entities = new List<WateringNeed>
            {
                new WateringNeed {Title="meow"},
                new WateringNeed {Title="meow2"},
            };

            var p = new List<WateringNeedDTO>();
            // Act
            var result = await _wateringNeedController!.Get(ids);
            if (result.Result is OkObjectResult okResult)
            {
                p = okResult.Value as List<WateringNeedDTO>;
            }

            // ClassicAssert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsAssignableFrom<List<WateringNeedDTO>>(p);
            ClassicAssert.AreEqual(entities.Count, p?.Count() ?? 0);
        }

        [Test, Order(3)]
        public async Task Post_ReturnsOkResult_WhenwateringNeedsChanged()
        {
            // Arrange
            var request = new List<RequestCommonDTO>
        {
            new RequestCommonDTO {
                Title = "meow"
            },
            new RequestCommonDTO {
                Title = "meow3"
            }
        };

            // Act
            var result = await _wateringNeedController!.Post(request);

            // ClassicAssert
            var badRequestResult = result as OkObjectResult;
            ClassicAssert.IsInstanceOf<OkResult>(result);

        }

        [Test, Order(4)]
        public async Task Delete_ReturnsOkResult_WhenwateringNeedsDeleted()
        {
            // Arrange
            var wateringNeedIds = new List<int> { 1, 2 };

            // Act
            var result = await _wateringNeedController!.Delete(wateringNeedIds);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }

        [Test, Order(5)]
        public async Task Delete_ReturnsBadRequest_WhenwateringNeedsNotDeleted()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };

            // Act
            var result = await _wateringNeedController!.Delete(ids);

            // ClassicAssert
            var badRequestResult = result as BadRequestObjectResult;
            ClassicAssert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult.");
            ClassicAssert.AreEqual("No watering needs were deleted!", badRequestResult!.Value);
        }
    }
}
