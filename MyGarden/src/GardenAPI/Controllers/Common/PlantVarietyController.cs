using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using EntityLibrary.Transfer.Common;
using GardenAPI.Data;
using GardenAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace GardenAPI.Controllers.Common
{
    [Route("api/plant_variety")]
    [ApiController]
    public class PlantVarietyController(PlantVarietyService dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private PlantVarietyService DataEntityService { get; } = dataEntityService;

        /// <summary>
        ///     Получить список видов растений.
        ///     Если идентификаторы не указаны, возвращается список со всеми типами сада пользователя.
        ///     Иначе возвращается список с указанными типами сада пользователя, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком видов растений.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantTypeDTO>>> Get([FromQuery] List<int> ids)
        {
            var plantTypes = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).PlantVarieties, ids)).Select(x => x.ToDTO<PlantVarietyDTO>()).ToList();
            return Ok(plantTypes);
        }

        /// <summary>
        ///     Сохранить список видов растений.
        /// </summary>
        /// <param name="entities">Список видов растений.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestCommonDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).PlantVarieties, entities.Select(x => x.ToEntity<PlantVariety>()).ToList());

            if (!status)
            {
                return BadRequest("No garden types were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить список видов растений.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).PlantVarieties, ids);

            if (!status)
            {
                return BadRequest("No garden types were deleted!");
            }

            return Ok();
        }
    }
}
