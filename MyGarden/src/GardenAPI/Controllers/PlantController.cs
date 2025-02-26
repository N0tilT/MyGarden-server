using EntitiesLibrary.Plants;
using EntitiesLibrary.Services;
using EntitiesLibrary.Transfer.Plant;
using GardenAPI.Data;
using GardenAPI.Service;
using GardenAPI.Service.Plants;
using Microsoft.AspNetCore.Mvc;

namespace GardenAPI.Controllers
{
    [Route("api/plant")]
    [ApiController]
    public class PlantController(IHasUserIdService<Plant> dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private PlantService DataEntityService { get; } = (PlantService)dataEntityService;

        /// <summary>
        ///     Получить список растений.
        ///     Если идентификаторы не указаны, возвращается список со всеми растениями.
        ///     Иначе возвращается список с указанными растениями, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком растений.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantDTO>>> Get([FromQuery] string userId, [FromBody] List<int> ids)
        {
            var plants = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).Plants, userId, ids)).Select(x => x.ToDTO()).ToList();
            return Ok(plants);
        }

        /// <summary>
        ///     Сохранить растения.
        /// </summary>
        /// <param name="entities">Список растений.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestPlantDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).Plants, entities.Select(x => x.ToEntity()).ToList());

            if (!status)
            {
                return BadRequest("No plants were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить растения.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).Plants, ids);

            if (!status)
            {
                return BadRequest("No plants were deleted!");
            }

            return Ok();
        }
    }
}
