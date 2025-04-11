using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using GardenAPI.Controllers.Common;
using GardenAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace GrowStageTest
{
    [TestFixture]
    internal class GrowStageControllerTests : CommonSetUp
    {
        internal required GrowStageController _growStageController;

        [OneTimeSetUp]
        public void SetUp()
        {
            _growStageController = new GrowStageController(_growStageService);
        }

        [Test, Order(1)]
        public async Task Post_ReturnsOkResult_WhengrowStagesSaved()
        {
            // Arrange
            var growStageRequest = new List<RequestCommonDTO>
        {
            new RequestCommonDTO {
                Title = "meow"
            },
            new RequestCommonDTO {
                Title = "meow2"
            }
        };
            // Act
            var result = await _growStageController!.Post(growStageRequest);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }
        [Test, Order(2)]
        public async Task Get_ReturnsOkResult_WithgrowStages()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };
            var entities = new List<GrowStage>
            {
                new GrowStage {Title="meow"},
                new GrowStage {Title="meow2"},
            };

            var p = new List<GrowStageDTO>();
            // Act
            var result = await _growStageController!.Get(ids);
            if (result.Result is OkObjectResult okResult)
            {
                p = okResult.Value as List<GrowStageDTO>;
            }

            // ClassicAssert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsAssignableFrom<List<GrowStageDTO>>(p);
            ClassicAssert.AreEqual(entities.Count, p?.Count() ?? 0);
        }

        [Test, Order(3)]
        public async Task Post_ReturnsOkResult_WhengrowStagesChanged()
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
            var result = await _growStageController!.Post(request);

            // ClassicAssert
            var badRequestResult = result as OkObjectResult;
            ClassicAssert.IsInstanceOf<OkResult>(result);

        }

        [Test, Order(4)]
        public async Task Delete_ReturnsOkResult_WhengrowStagesDeleted()
        {
            // Arrange
            var growStageIds = new List<int> { 1, 2 };

            // Act
            var result = await _growStageController!.Delete(growStageIds);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }

        [Test, Order(5)]
        public async Task Delete_ReturnsBadRequest_WhengrowStagesNotDeleted()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };

            // Act
            var result = await _growStageController!.Delete(ids);

            // ClassicAssert
            var badRequestResult = result as BadRequestObjectResult;
            ClassicAssert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult.");
            ClassicAssert.AreEqual("No stages were deleted!", badRequestResult!.Value);
        }
    }
}
