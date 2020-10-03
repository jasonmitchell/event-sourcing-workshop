using System;
using Kanban.Framework;

namespace Kanban.Domain.Cards.Events
{
    public class TaskOpened : Event
    {
        public string CardId { get; }
        public DateTime DateOpened { get; }
        public string Title { get; }

        public TaskOpened(string cardId, DateTime dateOpened, string title)
        {
            CardId = cardId;
            DateOpened = dateOpened;
            Title = title;
        }
    }
}