using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using GardenAPI.Data;
using GardenAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace GardenAPI.Controllers.Common
{
    [Route("api/garden_type")]
    [ApiController]
    public class GardenTypeController(GardenTypeService dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private GardenTypeService DataEntityService { get; } = dataEntityService;

        /// <summary>
        ///     Получить список типов сада пользователя.
        ///     Если идентификаторы не указаны, возвращается список со всеми типами сада пользователя.
        ///     Иначе возвращается список с указанными типами сада пользователя, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком типов сада пользователя.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GardenTypeDTO>>> Get([FromQuery] List<int> ids)
        {
            var groups = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).GardenTypes, ids)).Select(x => x.ToDTO<GardenTypeDTO>()).ToList();
            return Ok(groups);
        }

        /// <summary>
        ///     Сохранить список типов сада пользователя.
        /// </summary>
        /// <param name="entities">Список типов сада пользователя.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestCommonDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).GardenTypes, entities.Select(x => x.ToEntity<GardenType>()).ToList());

            if (!status)
            {
                return BadRequest("No garden types were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить список типов сада пользователя.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).GardenTypes, ids);

            if (!status)
            {
                return BadRequest("No garden types were deleted!");
            }

            return Ok();
        }
    }
}
