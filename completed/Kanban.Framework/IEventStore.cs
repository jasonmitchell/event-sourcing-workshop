using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kanban.Framework
{
    public interface IEventStore
    {
        /// <summary>
        /// Reads an event stream forwards from beginning to end.
        /// </summary>
        /// <param name="streamName">The stream name to read</param>
        /// <returns>The events recorded in the stream</returns>
        Task<IEnumerable<Event>> ReadStream(string streamName);
        
        /// <summary>
        /// Appends a set of events to the end of the specified stream.
        /// </summary>
        /// <param name="streamName">The stream to write to</param>
        /// <param name="expectedVersion">The version the stream is expected to be at</param>
        /// <param name="events">The set of events to write</param>
        /// <param name="correlationId">An identifier for this write operation</param>
        /// <remarks>
        /// The expectedVersion parameter is used as a concurrency check when writing
        /// to the event stream. If the version does not match then the aggregate must
        /// have been updated before this write has completed.
        /// </remarks>
        Task AppendToStream(string streamName, long expectedVersion, IEnumerable<Event> events, string correlationId);
    }
}