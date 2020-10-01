using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Kanban.Domain.Cards.Commands;
using Kanban.Domain.Cards.Events;
using Kanban.Tests.Fixtures;
using Kanban.Tests.Integration.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace Kanban.Tests.Integration.Api
{
    public class CardsApiTests : IClassFixture<EsdbFixture>, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly EsdbFixture _esdb;
        private readonly WebApplicationFactory<Startup> _factory;

        public CardsApiTests(EsdbFixture esdb, WebApplicationFactory<Startup> factory)
        {
            _esdb = esdb;
            _factory = factory;
        }
        
        private static string RandomId() => Guid.NewGuid().ToString();

        private static async Task<HttpResponseMessage> OpenTask(HttpClient client, string cardId)
        {
            var command = new OpenTask
            {
                Id = cardId,
                Title = Guid.NewGuid().ToString("N")
            };

            var response = await client.PostAsync("/tasks", command.AsJsonStringContent());
            return response;
        }

        private static async Task<HttpResponseMessage> AssignCard(HttpClient client, string cardId, string assignee)
        {
            var command = new AssignCard
            {
                Id = cardId,
                Assignee = assignee
            };

            var response = await client.PutAsync($"/cards/{cardId}/assignee", command.AsJsonStringContent());
            return response;
        }
        
        private static async Task<HttpResponseMessage> StartDevelopment(HttpClient client, string cardId)
        {
            var response = await client.PostAsync($"/cards/{cardId}/start", new StringContent(string.Empty));
            return response;
        }
        
        [Fact]
        public async Task OpensNewCard()
        {
            var client = _factory.CreateClient();

            var cardId = RandomId();
            var response = await OpenTask(client, cardId);
            response.EnsureSuccessStatusCode();

            var streamName = $"Card-{cardId}";
            await _esdb.AssertStreamExists(streamName);
            await _esdb.AssertEventTypes(streamName, new[]
            {
                typeof(TaskOpened)
            });
        }

        [Fact]
        public async Task AssignsCard()
        {
            var client = _factory.CreateClient();
            
            var cardId = RandomId();
            await OpenTask(client, cardId);

            var response = await AssignCard(client, cardId, "Jason Mitchell");
            response.EnsureSuccessStatusCode();
            
            var streamName = $"Card-{cardId}";
            await _esdb.AssertStreamExists(streamName);
            await _esdb.AssertEventTypes(streamName, new[]
            {
                typeof(TaskOpened),
                typeof(CardAssigned)
            });
        }
        
        [Fact]
        public async Task StartsDevelopment()
        {
            var client = _factory.CreateClient();
            
            var cardId = RandomId();
            await OpenTask(client, cardId);
            await AssignCard(client, cardId, "Jason Mitchell");

            var response = await StartDevelopment(client, cardId);
            response.EnsureSuccessStatusCode();
            
            var streamName = $"Card-{cardId}";
            await _esdb.AssertStreamExists(streamName);
            await _esdb.AssertEventTypes(streamName, new[]
            {
                typeof(TaskOpened),
                typeof(CardAssigned),
                typeof(DevelopmentStarted)
            });
        }

        [Fact]
        public async Task GetsCardActivity()
        {
            var client = _factory.CreateClient();
            
            var cardId = RandomId();
            await OpenTask(client, cardId);
            await AssignCard(client, cardId, "Jason Mitchell");
            await StartDevelopment(client, cardId);

            await Wait.UntilAsserted(async () =>
            {
                var response = await client.GetAsync($"/cards/{cardId}/activity");
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var activity = JsonConvert.DeserializeObject<string[]>(body);
                activity.Should().HaveCount(3);
            });
        }
    }
}