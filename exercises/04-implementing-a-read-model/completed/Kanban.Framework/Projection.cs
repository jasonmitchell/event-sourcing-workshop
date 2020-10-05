using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kanban.Framework
{
    /// <summary>
    /// Abstract base class for implementing projections
    /// </summary>
    public abstract class Projection
    {
        protected delegate Task Handler<in TEvent>(TEvent e) where TEvent : Event;
        
        private readonly Dictionary<Type, List<Handler<Event>>> _handlers = new Dictionary<Type, List<Handler<Event>>>();
        
        private Dictionary<string, Type> EventTypeMap { get; } = new Dictionary<string, Type>();
        
        /// <summary>
        /// Executes the project handlers for the provided event.
        /// </summary>
        /// <param name="e"></param>
        public async Task Handle(Event e)
        {
            var handlers = _handlers[e.GetType()];

            foreach (var handler in handlers)
            {
                await handler(e);
            }
        }
        
        /// <summary>
        /// Checks if the projection has any handlers which can handle
        /// the provided event type. 
        /// </summary>
        /// <param name="eventType"></param>
        public bool CanHandle(Type eventType)
        {
            return _handlers.ContainsKey(eventType);
        }

        /// <summary>
        /// Defines a handler for an event
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="TEvent"></typeparam>
        protected void When<TEvent>(Handler<TEvent> handler) where TEvent : Event
        {
            When(typeof(TEvent).Name, handler);
        }

        private void When<TEvent>(string eventType, Handler<TEvent> handler) where TEvent : Event
        {
            if (!_handlers.ContainsKey(typeof(TEvent)))
            {
                _handlers.Add(typeof(TEvent), new List<Handler<Event>>());
            }

            _handlers[typeof(TEvent)].Add(e => handler((TEvent)e));
            EventTypeMap[eventType] = typeof(TEvent);
        }
    }
}