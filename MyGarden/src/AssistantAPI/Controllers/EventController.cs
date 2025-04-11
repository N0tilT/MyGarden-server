using AssistantAPI.Data;
using EntitiesLibrary.Transfer.Event;
using GardenAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace AssistantAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController(EventService dataEntityService) : ControllerBase
    {
        /// <summary>
        ///     Сервис моделей.
        /// </summary>
        private EventService DataEntityService { get; } = dataEntityService;

        /// <summary>
        ///     Получить список событий.
        ///     Если идентификаторы не указаны, возвращается список со всеми событиями.
        ///     Иначе возвращается список с указанными событиями, либо пустой список.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции со списком событий.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDTO>>> Get([FromQuery] string userId, [FromBody] List<int> ids)
        {
            var events = (await DataEntityService.Get(((DataContext)DataEntityService.DataContext).Events, userId, ids)).Select(x => x.ToDTO()).ToList();
            return Ok(events);
        }

        /// <summary>
        ///     Сохранить события.
        /// </summary>
        /// <param name="entities">Список событий.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RequestEventDTO> entities)
        {
            var status = await DataEntityService.Set(((DataContext)DataEntityService.DataContext).Events, entities.Select(x => x.ToEntity()).ToList());

            if (!status)
            {
                return BadRequest("No events were saved!");
            }

            return Ok();
        }

        /// <summary>
        ///     Удалить события.
        /// </summary>
        /// <param name="ids">Список идентификаторов.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var status = await DataEntityService.Remove(((DataContext)DataEntityService.DataContext).Events, ids);

            if (!status)
            {
                return BadRequest("No events were deleted!");
            }

            return Ok();
        }
    }
}
