using Kanban.Framework;

namespace Kanban.Domain.Cards
{
    public class CardAggregateStore : EventSourcedAggregateStore<CardAggregate>
    {
        public CardAggregateStore(IEventStore eventStore) 
            : base(eventStore) { }

        public override string StreamName(string aggregateId) => $"Card-{aggregateId}";

        public override CardAggregate CreateAggregate(string aggregateId) => new CardAggregate(aggregateId);
    }
}