using EntitiesLibrary.Plants;
using EntitiesLibrary.Transfer.Group;
using GardenAPI.Controllers;
using GardenAPI.Tests;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace GroupTest
{
    [TestFixture]
    internal class GroupControllerTests : CommonSetUp
    {
        internal required GroupController _groupController;

        [OneTimeSetUp]
        public void SetUp()
        {
            _groupController = new GroupController(_groupService);
        }

        [Test, Order(1)]
        public async Task Post_ReturnsOkResult_WhenGroupsSaved()
        {
            // Arrange
            var groupRequest = new List<RequestGroupDTO>
        {
            new RequestGroupDTO {
                Title = "meow",
                UserId = "meow",
            },
            new RequestGroupDTO {
                Title = "meow2",
                UserId = "meow2",
            }
        };
            // Act
            var result = await _groupController!.Post(groupRequest);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }
        [Test, Order(2)]
        public async Task Get_ReturnsOkResult_WithGroups()
        {
            // Arrange
            var userId = "meow";
            var ids = new List<int> { 1, 2 };
            var entities = new List<Group>
            {
                new Group { UserId = "default", Title="default" },
                new Group { UserId = "meow", Title="meow" },
            };

            var p = new List<GroupDTO>();
            // Act
            var result = await _groupController!.Get(userId, ids);
            if (result.Result is OkObjectResult okResult)
            {
                p = okResult.Value as List<GroupDTO>;
            }

            // ClassicAssert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsAssignableFrom<List<GroupDTO>>(p);
            ClassicAssert.AreEqual(entities.Count, p?.Count() ?? 0);
        }

        [Test, Order(3)]
        public async Task Post_ReturnsOkResult_WhenGroupsChanged()
        {
            // Arrange
            var request = new List<RequestGroupDTO>
        {
            new RequestGroupDTO {
                Id = 1,
                Title = "meow",
                UserId = "meow"
            },
            new RequestGroupDTO {
                Id = 2,
                Title = "meow3",
                UserId = "meow2"
            }
        };

            // Act
            var result = await _groupController!.Post(request);

            // ClassicAssert
            var badRequestResult = result as OkObjectResult;
            ClassicAssert.IsInstanceOf<OkResult>(result);

        }

        [Test, Order(4)]
        public async Task Delete_ReturnsOkResult_WhenGroupsDeleted()
        {
            // Arrange
            var groupIds = new List<int> { 1, 2 };

            // Act
            var result = await _groupController!.Delete(groupIds);

            // ClassicAssert
            ClassicAssert.IsInstanceOf<OkResult>(result);
        }

        [Test, Order(5)]
        public async Task Delete_ReturnsBadRequest_WhenGroupsNotDeleted()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };

            // Act
            var result = await _groupController!.Delete(ids);

            // ClassicAssert
            var badRequestResult = result as BadRequestObjectResult;
            ClassicAssert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult.");
            ClassicAssert.AreEqual("No groups were deleted!", badRequestResult!.Value);
        }
    }
}