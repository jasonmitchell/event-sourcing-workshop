using System.Threading.Tasks;
using Kanban.Domain.Cards.Commands;
using Kanban.Domain.Cards.Queries;
using Kanban.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kanban.Controllers
{
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly ICommandHandler<OpenTask> _openTaskHandler;
        private readonly ICommandHandler<AssignCard> _assignCardHandler;
        private readonly ICommandHandler<StartDevelopment> _startDevelopmentHandler;
        private readonly ICardActivityRepository _repository;

        public CardsController(ICommandHandler<OpenTask> openTaskHandler,
                               ICommandHandler<AssignCard> assignCardHandler,
                               ICommandHandler<StartDevelopment> startDevelopmentHandler,
                               ICardActivityRepository repository)
        {
            _openTaskHandler = openTaskHandler;
            _assignCardHandler = assignCardHandler;
            _startDevelopmentHandler = startDevelopmentHandler;
            _repository = repository;
        }
        
        [HttpPost]
        [Route("tasks")]
        public async Task<IActionResult> CreateTask(OpenTask command)
        {
            await _openTaskHandler.Handle(command);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut]
        [Route("cards/{cardId}/assignee")]
        public async Task<IActionResult> Assign(AssignCard command)
        {
            await _assignCardHandler.Handle(command);
            return NoContent();
        }
        
        [HttpPost]
        [Route("cards/{cardId}/start")]
        public async Task<IActionResult> StartDevelopment(string cardId)
        {
            var command = new StartDevelopment
            {
                Id = cardId
            };
            
            await _startDevelopmentHandler.Handle(command);
            return NoContent();
        }

        [HttpGet]
        [Route("cards/{cardId}/activity")]
        public async Task<IActionResult> GetActivity(string cardId)
        {
            var activity = await _repository.GetActivityForCard(cardId);
            if (activity == null)
            {
                return NotFound();
            }

            return Ok(activity);
        }
    }
}