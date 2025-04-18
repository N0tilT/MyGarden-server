using EntitiesLibrary.Common;
using EntitiesLibrary.Transfer.Common;
using EntityLibrary.Transfer.Common;
using GardenAPI.Data;
using GardenAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace GardenAPI.Controllers.Common
{
    [Route("api/plant_type")]
    [ApiController]
    public class PlantTypeController(PlantTypeService dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private PlantTypeService DataEntityService { get; } = dataEntityService;

        /// <summary>
        ///     Получить список типов растений.
        ///     Если идентификаторы не указаны, возвращается список со всеми типами сада пользователя.
        ///     Иначе возвращается список с указанными типами сада пользователя, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком типов растений.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantTypeDTO>>> Get([FromQuery] List<int> ids)
        {
            var plantTypes = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).PlantTypes, ids)).Select(x => x.ToDTO<PlantTypeDTO>()).ToList();
            return Ok(plantTypes);
        }

        /// <summary>
        ///     Сохранить список типов растений.
        /// </summary>
        /// <param name="entities">Список типов растений.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestCommonDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).PlantTypes, entities.Select(x => x.ToEntity<PlantType>()).ToList());

            if (!status)
            {
                return BadRequest("No garden types were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить список типов растений.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).PlantTypes, ids);

            if (!status)
            {
                return BadRequest("No garden types were deleted!");
            }

            return Ok();
        }
    }
}
