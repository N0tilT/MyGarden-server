using EntitiesLibrary.Transfer.Garden;
using GardenAPI.Data;
using GardenAPI.Service.Gardens;
using Microsoft.AspNetCore.Mvc;

namespace GardenAPI.Controllers
{
    [Route("api/g")]
    [ApiController]
    public class GardenController(GardenService dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private GardenService DataEntityService { get; } = dataEntityService;

        /// <summary>
        ///     Получить список садов пользователя.
        ///     Если идентификаторы не указаны, возвращается список со всеми садами.
        ///     Иначе возвращается список с указанными садами, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком садов пользователя.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GardenDTO>>> Get([FromQuery] string userId, [FromBody] List<int> ids)
        {
            var groups = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).Gardens, userId, ids)).Select(x => x.ToDTO()).ToList();
            return Ok(groups);
        }

        /// <summary>
        ///     Сохранить сады пользователя.
        /// </summary>
        /// <param name="entities">Список садов.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestGardenDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).Gardens, entities.Select(x => x.ToEntity()).ToList());

            if (!status)
            {
                return BadRequest("No gardens were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить сады пользователя.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).Gardens, ids);

            if (!status)
            {
                return BadRequest("No gardens were deleted!");
            }

            return Ok();
        }
    }
}
