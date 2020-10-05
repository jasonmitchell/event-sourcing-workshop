using System.Collections.Generic;
using System.Linq;

namespace Kanban.Framework
{
    /// <summary>
    /// Abstract base class for implementing event sourced aggregates
    /// </summary>
    public abstract class EventSourcedAggregateRoot
    {
        private readonly List<Event> _changes = new List<Event>();
        
        /// <summary>
        /// The id of the the aggregate.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// The current version of the aggregate.
        /// </summary>
        /// <remarks>
        /// Aggregate version is determined by the number of events
        /// in the history when an aggregate is loaded.
        /// </remarks>
        public int Version { get; private set; } = -1;

        /// <summary>
        /// Provides access to the changes recorded by the aggregate.
        /// </summary>
        public IEnumerable<Event> GetChanges() => _changes.AsEnumerable();
        
        /// <summary>
        /// Clears the changes recorded by the aggregate.
        /// </summary>
        public void ClearChanges() => _changes.Clear();

        protected EventSourcedAggregateRoot(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Reconstructs aggregate state by applying each event in
        /// the aggregate history.
        /// </summary>
        /// <param name="history">
        /// The full history of events previously recorded by this
        /// aggregate instance.
        /// </param>
        public void Load(IEnumerable<Event> history)
        {
            foreach (var @event in history)
            {
                Apply(@event);
                Version++;
            }
        }

        /// <summary>
        /// Records an event as a change to this aggregate and
        /// applies the event to update the state.
        /// </summary>
        /// <param name="event"></param>
        protected void Raise(Event @event)
        {
            _changes.Add(@event);
            Apply(@event);
        }
        
        /// <summary>
        /// Implemented by aggregates to update aggregate state.
        /// </summary>
        /// <param name="event"></param>
        protected abstract void Apply(Event @event);
    }
}