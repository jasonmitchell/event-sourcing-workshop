using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EventStore.Client;
using FluentAssertions;
using Kanban.Framework;
using Xunit;

namespace Kanban.Tests.Fixtures
{
    public class EsdbFixture
    {
        public EventStoreClient Client { get; }

        public EsdbFixture()
        {
            Client = CreateEventStoreClient();
        }
        
        private EventStoreClient CreateEventStoreClient()
        {
            return new EventStoreClient(new EventStoreClientSettings
            {
                ConnectivitySettings =
                {
                    Address = new Uri("http://localhost:2113"),
                },
                DefaultCredentials = new UserCredentials("admin", "changeit")
            });
        }

        public async Task SeedStream(string streamName, IEnumerable<Event> events)
        {
            var correlationId = Guid.NewGuid().ToString();
            var eventData = events.Select(e =>
            {
                var metadata = new Dictionary<string, string>
                {
                    {"$correlationId", correlationId}
                };
                
                var eventBytes = Encoding.UTF8.GetBytes(EventSerializer.Serialize(e));
                var metadataBytes = Encoding.UTF8.GetBytes(EventSerializer.SerializeMetadata(metadata));
                
                return new EventData(Uuid.NewUuid(), e.GetType().Name, eventBytes, metadataBytes);
            });
            
            await Client.AppendToStreamAsync(streamName, StreamState.NoStream, eventData);
        }
        
        public async Task AssertStreamExists(string streamName)
        {
            var result = Client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, 1);
            Assert.Equal(ReadState.Ok, await result.ReadState);
        }

        public async Task AssertEventTypes(string streamName, Type[] types)
        {
            var result = Client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);

            var actualEventTypes = (await result.ToListAsync())
                                   .Select(EventSerializer.Deserialize)
                                   .Select(e => e.GetType())
                                   .ToList();

            types.Should().BeEquivalentTo(actualEventTypes, opt => opt.WithStrictOrdering());
        }

        public async Task AssertEvents(string streamName, IEnumerable<Event> events)
        {
            var result = Client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);

            var actualEvents = (await result.ToListAsync())
                               .Select(EventSerializer.Deserialize)
                               .Select(e => (object)e)
                               .ToList();

            events.Should().BeEquivalentTo(actualEvents, opt => opt.WithStrictOrdering());
        }
    }
}