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
    public class OpenTaskTests : IClassFixture<EsdbFixture>
    {
        private readonly EsdbFixture _esdb;
        private readonly Handlers _handlers;

        public OpenTaskTests(EsdbFixture esdb)
        {
            _esdb = esdb;
            
            var eventStore = new EsdbEventStore(esdb.Client);
            var aggregateStore = new CardAggregateStore(eventStore);
            _handlers = new Handlers(aggregateStore);
        }
        
        [Fact]
        public async Task OpensNewCard()
        {
            var cardId = Guid.NewGuid().ToString();
            var command = new OpenTask
            {
                Id = cardId,
                Title = Guid.NewGuid().ToString("N")
            };

            await _handlers.Handle(command);

            var streamName = $"Card-{cardId}";
            await _esdb.AssertStreamExists(streamName);
            await _esdb.AssertEventTypes(streamName, new[]
            {
                typeof(TaskOpened)
            });
        }
    }
}