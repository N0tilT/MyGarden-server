using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using GardenAPI.Controllers.Common;
using GardenAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace GardenTypeTest
{
    [TestFixture]
    internal class GardenTypeControllerTests : CommonSetUp
    {
        internal required GardenTypeController _gardenTypeController;

        [OneTimeSetUp]
        public void SetUp()
        {
            _gardenTypeController = new GardenTypeController(_gardenTypeService);
        }

        [Test, Order(1)]
        public async Task Post_ReturnsOkResult_WhengardenTypesSaved()
        {
            // Arrange
            var gardenTypeRequest = new List<RequestCommonDTO>
        {
            new RequestCommonDTO {
                Title = "meow"
            },
            new RequestCommonDTO {
                Title = "meow2"
            }
        };
            // Act
            var result = await _gardenTypeController!.Post(gardenTypeRequest);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }
        [Test, Order(2)]
        public async Task Get_ReturnsOkResult_WithgardenTypes()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };
            var entities = new List<GardenType>
            {
                new GardenType {Title="meow"},
                new GardenType {Title="meow2"},
            };

            var p = new List<GardenTypeDTO>();
            // Act
            var result = await _gardenTypeController!.Get(ids);
            if (result.Result is OkObjectResult okResult)
            {
                p = okResult.Value as List<GardenTypeDTO>;
            }

            // ClassicAssert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsAssignableFrom<List<GardenTypeDTO>>(p);
            ClassicAssert.AreEqual(entities.Count, p?.Count() ?? 0);
        }

        [Test, Order(3)]
        public async Task Post_ReturnsOkResult_WhengardenTypesChanged()
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
            var result = await _gardenTypeController!.Post(request);

            // ClassicAssert
            var badRequestResult = result as OkObjectResult;
            ClassicAssert.IsInstanceOf<OkResult>(result);

        }

        [Test, Order(4)]
        public async Task Delete_ReturnsOkResult_WhengardenTypesDeleted()
        {
            // Arrange
            var gardenTypeIds = new List<int> { 1, 2 };

            // Act
            var result = await _gardenTypeController!.Delete(gardenTypeIds);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }

        [Test, Order(5)]
        public async Task Delete_ReturnsBadRequest_WhengardenTypesNotDeleted()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };

            // Act
            var result = await _gardenTypeController!.Delete(ids);

            // ClassicAssert
            var badRequestResult = result as BadRequestObjectResult;
            ClassicAssert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult.");
            ClassicAssert.AreEqual("No garden types were deleted!", badRequestResult!.Value);
        }
    }
}
