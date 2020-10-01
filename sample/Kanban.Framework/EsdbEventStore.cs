using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.Client;

namespace Kanban.Framework
{
    public class EsdbEventStore : IEventStore
    {
        private readonly EventStoreClient _client;

        public EsdbEventStore(EventStoreClient client)
        {
            _client = client;
        }
        
        public async Task<IEnumerable<Event>> ReadStream(string streamName)
        {
            var result = _client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);
            
            var readState = await result.ReadState;
            if (readState == ReadState.StreamNotFound)
            {
                return new List<Event>();
            }

            var events = (await result.ToListAsync())
                         .Select(EventSerializer.Deserialize)
                         .ToList();

            return events;
        }

        public async Task AppendToStream(string streamName, long expectedVersion, IEnumerable<Event> events, string correlationId)
        {
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
            
            var expectedRevision = StreamRevision.FromInt64(expectedVersion);
            await _client.AppendToStreamAsync(streamName, expectedRevision, eventData);
        }
    }
}