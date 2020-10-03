using System;
using Kanban.Framework;

namespace Kanban.Domain.Cards.Events
{
    public class DevelopmentStarted : Event
    {
        public string CardId { get; }
        public DateTime DateStarted { get; }

        public DevelopmentStarted(string cardId, DateTime dateStarted)
        {
            CardId = cardId;
            DateStarted = dateStarted;
        }
    }
}