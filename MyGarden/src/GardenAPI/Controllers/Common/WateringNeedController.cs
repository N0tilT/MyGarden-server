
using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using GardenAPI.Data;
using GardenAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace GardenAPI.Controllers.Common
{
    [Route("api/watering")]
    [ApiController]
    public class WateringNeedController(WateringNeedService dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private WateringNeedService DataEntityService { get; } = dataEntityService;

        /// <summary>
        ///     Получить список уровней полива.
        ///     Если идентификаторы не указаны, возвращается список со всеми уровнями.
        ///     Иначе возвращается список с указанными уровнями, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком уровней полива.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WateringNeedDTO>>> Get([FromQuery] List<int> ids)
        {
            var groups = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).WateringNeeds, ids)).Select(x => x.ToDTO<WateringNeedDTO>()).ToList();
            return Ok(groups);
        }

        /// <summary>
        ///     Сохранить уровни полива.
        /// </summary>
        /// <param name="entities">Список уровней полива.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestCommonDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).WateringNeeds, entities.Select(x => x.ToEntity<WateringNeed>()).ToList());

            if (!status)
            {
                return BadRequest("No watering needs were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить уровни полива.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).WateringNeeds, ids);

            if (!status)
            {
                return BadRequest("No watering needs were deleted!");
            }

            return Ok();
        }
    }
}
