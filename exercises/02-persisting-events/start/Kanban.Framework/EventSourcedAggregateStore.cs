using System;
using System.Threading.Tasks;

namespace Kanban.Framework
{
    /// <summary>
    /// Abstract base class for loading and saving event sourced aggregates.
    /// </summary>
    /// <remarks>
    /// Each aggregate will have an aggregate store derived from this type.
    /// </remarks>
    /// <typeparam name="TAggregate"></typeparam>
    public abstract class EventSourcedAggregateStore<TAggregate> where TAggregate : EventSourcedAggregateRoot
    {
        private readonly IEventStore _eventStore;

        protected EventSourcedAggregateStore(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }
        
        /// <param name="aggregateId"></param>
        /// <returns>The stream name for this aggregate</returns>
        public abstract string StreamName(string aggregateId);
        
        /// <summary>
        /// A factory method for constructing aggregates.
        /// </summary>
        /// <remarks>
        /// The aggregate created by this method will not have
        /// been rehydrated from its history.
        /// </remarks>
        /// <param name="aggregateId"></param>
        /// <returns>A new aggregate instance.</returns>
        public abstract TAggregate CreateAggregate(string aggregateId);

        /// <summary>
        /// Loads an aggregate with the specified id and replays its
        /// history.
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <returns>An aggregate with populate state</returns>
        public async Task<TAggregate> Load(string aggregateId)
        {
            var streamName = StreamName(aggregateId);
            var aggregate = CreateAggregate(aggregateId);

            var history = await _eventStore.ReadStream(streamName);
            aggregate.Load(history);

            return aggregate;
        }
        
        /// <summary>
        /// Extracts changes to the provided aggregate and appends them
        /// to an event stream.
        /// </summary>
        /// <param name="aggregate"></param>
        public async Task Save(TAggregate aggregate)
        {
            var streamName = StreamName(aggregate.Id);
            var changes = aggregate.GetChanges();

            await _eventStore.AppendToStream(streamName, aggregate.Version, changes, Guid.NewGuid().ToString());
            aggregate.ClearChanges();
        }
    }
}