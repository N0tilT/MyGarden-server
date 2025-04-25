using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using GardenAPI.Data;
using GardenAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace GardenAPI.Controllers.Common
{
    [Route("api/stage")]
    [ApiController]
    public class GrowStageController(GrowStageService dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private GrowStageService DataEntityService { get; } = dataEntityService;

        /// <summary>
        ///     Получить список стадий роста растения.
        ///     Если идентификаторы не указаны, возвращается список со всеми стадиями.
        ///     Иначе возвращается список с указанными стадиями, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком стадий роста растения.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GrowStageDTO>>> Get([FromQuery] List<int> ids)
        {
            var groups = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).GrowStages, ids)).Select(x => x.ToDTO<GrowStageDTO>()).ToList();
            return Ok(groups);
        }

        /// <summary>
        ///     Сохранить стадии роста растения.
        /// </summary>
        /// <param name="entities">Список стадий роста растения.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestCommonDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).GrowStages, entities.Select(x => x.ToEntity<GrowStage>()).ToList());

            if (!status)
            {
                return BadRequest("No stages were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить стадии роста растения.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).GrowStages, ids);

            if (!status)
            {
                return BadRequest("No stages were deleted!");
            }

            return Ok();
        }
    }
}
