using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Kanban.Domain.Cards;
using Kanban.Domain.Cards.Events;
using Kanban.Framework;
using Kanban.Tests.Fixtures;
using Xunit;

namespace Kanban.Tests.Integration
{
    public class CardAggregateStoreTests : IClassFixture<EsdbFixture>
    {
        private readonly EsdbFixture _esdb;
        private readonly CardAggregateStore _aggregateStore;
        
        public CardAggregateStoreTests(EsdbFixture esdb)
        {
            _esdb = esdb;
            var eventStore = new EsdbEventStore(esdb.Client);
            _aggregateStore = new CardAggregateStore(eventStore);
        }
        
        private static string RandomId() => Guid.NewGuid().ToString();

        [Fact]
        public async Task SavesCardAggregateChangesToStream()
        {
            var cardId = RandomId();
            var card = new CardAggregate(cardId);
            card.OpenTask("Test");
            card.AssignTo("Jason Mitchell");
            card.StartDevelopment();

            var expectedChanges = card.GetChanges().ToArray();
            await _aggregateStore.Save(card);

            var streamName = $"Card-{cardId}";
            await _esdb.AssertStreamExists(streamName);
            await _esdb.AssertEvents(streamName, expectedChanges);
        }

        [Fact]
        public async Task LoadsCardAggregateFromStream()
        {
            var cardId = RandomId();
            var streamName = $"Card-{cardId}";

            await _esdb.SeedStream(streamName, new Event[]
            {
                new TaskOpened(cardId, DateTime.UtcNow.AddDays(-1), "Test task"),
                new CardAssigned(cardId, null, "Jason Mitchell"),
                new DevelopmentStarted(cardId, DateTime.UtcNow)
            });

            var card = await _aggregateStore.Load(cardId);
            card.Version.Should().Be(2);
            
            // Sanity check to ensure state is reconstructed
            card.Title.Should().Be("Test task");
        }
    }
}