using System;
using Kanban.Domain.Cards.Events;
using Kanban.Domain.Cards.Exceptions;
using Kanban.Framework;

namespace Kanban.Domain.Cards
{
    public class CardAggregate : EventSourcedAggregateRoot
    {
        public CardType Type { get; private set; }
        public CardStatus Status { get; private set; } = CardStatus.ToDo;
        public DateTime DateOpened { get; private set; }
        
        public string Title { get; private set; }
        public string Assignee { get; private set; }

        public CardAggregate(string id) 
            : base(id) { }

        public void OpenTask(string title)
        {
            Ensure.NotNullOrEmpty(nameof(title), title);
            Raise(new TaskOpened(Id, DateTime.UtcNow, title));
        }
        
        public void AssignTo(string newAssignee)
        {
            Ensure.NotNullOrEmpty(nameof(newAssignee), newAssignee);

            if (newAssignee != Assignee)
            {
                Raise(new CardAssigned(Id, Assignee, newAssignee));
            }
        }
        
        public void StartDevelopment()
        {
            if (string.IsNullOrWhiteSpace(Assignee))
            {
                throw new AssigneeIsRequiredException();
            }
            
            if (Status != CardStatus.InProgress)
            {
                Raise(new DevelopmentStarted(Id, DateTime.UtcNow));
            }
        }

        protected override void Apply(Event @event)
        {
            switch (@event)
            {
                case TaskOpened e:
                    Type = CardType.Task;
                    Title = e.Title;
                    DateOpened = e.DateOpened;
                    break;
                
                case CardAssigned e:
                    Assignee = e.CurrentAssignee;
                    break;
                
                case DevelopmentStarted _:
                    Status = CardStatus.InProgress;
                    break;
            }
        }
    }
}