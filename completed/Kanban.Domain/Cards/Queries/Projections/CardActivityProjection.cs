using Kanban.Domain.Cards.Events;
using Kanban.Framework;

namespace Kanban.Domain.Cards.Queries.Projections
{
    public class CardActivityProjection : Projection
    {
        public CardActivityProjection(ICardActivityRepository repository)
        {
            When<TaskOpened>(async e =>
            {
                await repository.AppendActivityToCard(e.CardId, $"Task '{e.Title}' was opened");
            });

            When<CardAssigned>(async e =>
            {
                await repository.AppendActivityToCard(e.CardId, $"Card was assigned to {e.CurrentAssignee}");
            });

            When<DevelopmentStarted>(async e =>
            {
                await repository.AppendActivityToCard(e.CardId, "Development was started");
            });
        }
    }
}