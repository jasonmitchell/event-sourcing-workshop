using System.Threading.Tasks;
using Kanban.Framework;

namespace Kanban.Domain.Cards.Commands
{
    public class Handlers : ICommandHandler<OpenTask>, ICommandHandler<AssignCard>,
                            ICommandHandler<StartDevelopment>
    {
        private readonly CardAggregateStore _store;

        public Handlers(CardAggregateStore store)
        {
            _store = store;
        }
        
        public async Task Handle(OpenTask command)
        {
            var card = new CardAggregate(command.Id);
            card.OpenTask(command.Title);

            await _store.Save(card);
        }

        public async Task Handle(AssignCard command)
        {
            var card = await _store.Load(command.Id);
            card.AssignTo(command.Assignee);

            await _store.Save(card);
        }

        public async Task Handle(StartDevelopment command)
        {
            var card = await _store.Load(command.Id);
            card.StartDevelopment();

            await _store.Save(card);
        }
    }
}