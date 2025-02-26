using EntitiesLibrary.Transfer.Group;
using GardenAPI.Data;
using GardenAPI.Service.Plants;
using Microsoft.AspNetCore.Mvc;

namespace GardenAPI.Controllers
{
    [Route("api/group")]
    [ApiController]
    public class GroupController(GroupService dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private GroupService DataEntityService { get; } = dataEntityService;

        /// <summary>
        ///     Получить список групп растений.
        ///     Если идентификаторы не указаны, возвращается список со всеми группами растений.
        ///     Иначе возвращается список с указанными группами, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком групп растений.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDTO>>> Get([FromQuery] string userId, [FromBody] List<int> ids)
        {
            var groups = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).Groups, userId, ids)).Select(x => x.ToDTO()).ToList();
            return Ok(groups);
        }

        /// <summary>
        ///     Сохранить группы растений.
        /// </summary>
        /// <param name="entities">Список групп.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestGroupDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).Groups, entities.Select(x => x.ToEntity()).ToList());

            if (!status)
            {
                return BadRequest("No groups were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить группы растений.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).Groups, ids);

            if (!status)
            {
                return BadRequest("No groups were deleted!");
            }

            return Ok();
        }
    }
}
