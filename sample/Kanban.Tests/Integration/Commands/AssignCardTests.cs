using System;
using System.Threading.Tasks;
using Kanban.Domain.Cards;
using Kanban.Domain.Cards.Commands;
using Kanban.Domain.Cards.Events;
using Kanban.Framework;
using Kanban.Tests.Fixtures;
using Xunit;

namespace Kanban.Tests.Integration.Commands
{
    public class AssignCardTests : IClassFixture<EsdbFixture>
    {
        private readonly EsdbFixture _esdb;
        private readonly Handlers _handlers;

        public AssignCardTests(EsdbFixture esdb)
        {
            _esdb = esdb;
            
            var eventStore = new EsdbEventStore(esdb.Client);
            var aggregateStore = new CardAggregateStore(eventStore);
            _handlers = new Handlers(aggregateStore);
        }
        
        [Fact]
        public async Task AssignsCardToPerson()
        {
            var cardId = Guid.NewGuid().ToString();
            var streamName = $"Card-{cardId}";

            await _esdb.SeedStream(streamName, new[]
            {
                new TaskOpened(cardId, DateTime.UtcNow.AddHours(-1), "Test")
            });
            
            await _handlers.Handle(new AssignCard
            {
                Id = cardId,
                Assignee = "Jason Mitchell"
            });
            
            await _esdb.AssertEventTypes(streamName, new[]
            {
                typeof(TaskOpened),
                typeof(CardAssigned)
            });
        }
    }
}