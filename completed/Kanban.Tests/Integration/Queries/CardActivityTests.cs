using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kanban.Domain.Cards.Events;
using Kanban.Domain.Cards.Queries;
using Kanban.Domain.Cards.Queries.Projections;
using Kanban.Framework;
using Kanban.Tests.Fixtures;
using Kanban.Tests.Integration.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kanban.Tests.Integration.Queries
{
    public class CardActivityTests : IClassFixture<EsdbFixture>, IDisposable
    {
        private readonly EsdbFixture _esdb;
        private readonly ICardActivityRepository _repository;
        private readonly ProjectionRunner _runner;
        
        public CardActivityTests(EsdbFixture esdb)
        {
            _esdb = esdb;
            _repository = new InMemoryCardActivityRepository();
            _runner = new ProjectionRunner(esdb.Client, new NullLogger<ProjectionRunner>(), new Projection[]
            {
                new CardActivityProjection(_repository)
            });
        }

        [Fact]
        public async Task GeneratesActivityStreamForCard()
        {
            var cardId = Guid.NewGuid().ToString();
            var streamName = $"Card-{cardId}";

            await _esdb.SeedStream(streamName, new Event[]
            {
                new TaskOpened(cardId, DateTime.UtcNow.AddHours(-1), "Test"),
                new CardAssigned(cardId, null, "Jason Mitchell"),
                new DevelopmentStarted(cardId, DateTime.UtcNow) 
            });

            await _runner.Start(CancellationToken.None);

            await Wait.UntilAsserted(async () =>
            {
                var activity = await _repository.GetActivityForCard(cardId);
                activity.Should().BeEquivalentTo(new[]
                {
                    "Task 'Test' was opened",
                    "Card was assigned to Jason Mitchell",
                    "Development was started"
                }, opt => opt.WithStrictOrdering());
            });

        }

        public void Dispose()
        {
            _runner?.Dispose();
        }
    }
}