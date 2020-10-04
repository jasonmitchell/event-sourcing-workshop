using Kanban.Framework;

namespace Kanban.Domain.Cards.Events
{
    public class CardAssigned : Event
    {
        public string CardId { get; }
        public string PreviousAssignee { get; }
        public string CurrentAssignee { get; }

        public CardAssigned(string cardId, string previousAssignee, string currentAssignee)
        {
            CardId = cardId;
            PreviousAssignee = previousAssignee;
            CurrentAssignee = currentAssignee;
        }
    }
}