using System.Collections.Generic;
using System.Text;
using EventStore.Client;
using Newtonsoft.Json;

namespace Kanban.Framework
{
    public static class EventSerializer
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        /// <summary>
        /// Serializes an Event to a JSON string.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>The event JSON</returns>
        public static string Serialize(Event e)
        {
           return JsonConvert.SerializeObject(e, SerializerSettings);
        }

        /// <summary>
        /// Serializes event metadata to a JSON string.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns>The event metadata JSON</returns>
        public static string SerializeMetadata(Dictionary<string, string> metadata)
        {
            return JsonConvert.SerializeObject(metadata, SerializerSettings);
        }
        
        /// <summary>
        /// Deserializes an EventStoreDB event to a CLR event type.
        /// </summary>
        /// <param name="resolvedEvent"></param>
        public static Event Deserialize(ResolvedEvent resolvedEvent)
        {
            var body = Encoding.UTF8.GetString(resolvedEvent.Event.Data.ToArray());
            var e = JsonConvert.DeserializeObject<Event>(body, SerializerSettings);

            return e;
        }
    }
}