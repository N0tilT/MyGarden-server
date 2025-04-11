using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using GardenAPI.Data;
using GardenAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace GardenAPI.Controllers.Common
{
    [Route("api/light")]
    [ApiController]
    public class LightNeedController(LightNeedService dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private LightNeedService DataEntityService { get; } = dataEntityService;

        /// <summary>
        ///     Получить список уровней освещённости.
        ///     Если идентификаторы не указаны, возвращается список со всеми уровнями.
        ///     Иначе возвращается список с указанными уровнями, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком уровней освещённости.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LightNeedDTO>>> Get([FromQuery] List<int> ids)
        {
            var groups = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).LightNeeds, ids)).Select(x => x.ToDTO<LightNeedDTO>()).ToList();
            return Ok(groups);
        }

        /// <summary>
        ///     Сохранить уровни освещённости.
        /// </summary>
        /// <param name="entities">Список уровней освещённости.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestCommonDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).LightNeeds, entities.Select(x => x.ToEntity<LightNeed>()).ToList());

            if (!status)
            {
                return BadRequest("No light needs were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить уровни освещённости.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).LightNeeds, ids);

            if (!status)
            {
                return BadRequest("No light needs were deleted!");
            }

            return Ok();
        }
    }
}
