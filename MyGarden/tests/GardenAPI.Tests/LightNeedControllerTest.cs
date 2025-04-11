using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using GardenAPI.Controllers.Common;
using GardenAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LightNeedTest
{
    [TestFixture]
    internal class LightNeedControllerTests : CommonSetUp
    {
        internal required LightNeedController _lightNeedController;

        [OneTimeSetUp]
        public void SetUp()
        {
            _lightNeedController = new LightNeedController(_lightNeedService);
        }

        [Test, Order(1)]
        public async Task Post_ReturnsOkResult_WhenlightNeedsSaved()
        {
            // Arrange
            var lightNeedRequest = new List<RequestCommonDTO>
        {
            new RequestCommonDTO {
                Title = "meow"
            },
            new RequestCommonDTO {
                Title = "meow2"
            }
        };
            // Act
            var result = await _lightNeedController!.Post(lightNeedRequest);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }
        [Test, Order(2)]
        public async Task Get_ReturnsOkResult_WithlightNeeds()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };
            var entities = new List<LightNeed>
            {
                new LightNeed {Title="meow"},
                new LightNeed {Title="meow2"},
            };

            var p = new List<LightNeedDTO>();
            // Act
            var result = await _lightNeedController!.Get(ids);
            if (result.Result is OkObjectResult okResult)
            {
                p = okResult.Value as List<LightNeedDTO>;
            }

            // ClassicAssert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsAssignableFrom<List<LightNeedDTO>>(p);
            ClassicAssert.AreEqual(entities.Count, p?.Count() ?? 0);
        }

        [Test, Order(3)]
        public async Task Post_ReturnsOkResult_WhenlightNeedsChanged()
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
            var result = await _lightNeedController!.Post(request);

            // ClassicAssert
            var badRequestResult = result as OkObjectResult;
            ClassicAssert.IsInstanceOf<OkResult>(result);

        }

        [Test, Order(4)]
        public async Task Delete_ReturnsOkResult_WhenlightNeedsDeleted()
        {
            // Arrange
            var lightNeedIds = new List<int> { 1, 2 };

            // Act
            var result = await _lightNeedController!.Delete(lightNeedIds);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }

        [Test, Order(5)]
        public async Task Delete_ReturnsBadRequest_WhenlightNeedsNotDeleted()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };

            // Act
            var result = await _lightNeedController!.Delete(ids);

            // ClassicAssert
            var badRequestResult = result as BadRequestObjectResult;
            ClassicAssert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult.");
            ClassicAssert.AreEqual("No light needs were deleted!", badRequestResult!.Value);
        }
    }
}
