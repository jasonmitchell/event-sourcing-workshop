using System;
using FluentAssertions;
using Kanban.Domain.Cards;
using Kanban.Domain.Cards.Events;
using Kanban.Domain.Cards.Exceptions;
using Kanban.Framework;
using Xunit;

namespace Kanban.Tests.Domain
{
    public class CardAggregateTests
    {
        private static string RandomId() => Guid.NewGuid().ToString();
        
        [Fact]
        public void TaskIsOpened()
        {
            var card = new CardAggregate(RandomId());
            card.OpenTask("Test");

            var changes = card.GetChanges();
            changes.Should().BeEquivalentTo(new[]
            {
                new TaskOpened(card.Id, DateTime.UtcNow, "Test"),
            }, opt => opt.Excluding(x => x.DateOpened).WithStrictOrdering());
        }

        [Fact]
        public void HandlesTaskOpened()
        {
            var cardId = RandomId();
            var dateOpened = DateTime.UtcNow;
            var card = new CardAggregate(cardId);
            card.Load(new[]
            {
                new TaskOpened(cardId, dateOpened, "Test") 
            });

            card.Type.Should().Be(CardType.Task);
            card.DateOpened.Should().Be(dateOpened);
            card.Status.Should().Be(CardStatus.ToDo);
            card.Title.Should().Be("Test");
        }

        [Fact]
        public void CardIsAssigned()
        {
            var cardId = RandomId();
            var card = new CardAggregate(cardId);
            card.Load(new[]
            {
                new TaskOpened(cardId, DateTime.UtcNow, "Test") 
            });

            card.AssignTo("Jason Mitchell");
            
            var changes = card.GetChanges();
            changes.Should().BeEquivalentTo(new CardAssigned(card.Id, null, "Jason Mitchell"));
        }
        
        [Fact]
        public void HandlesCardAssigned()
        {
            var cardId = RandomId();
            var card = new CardAggregate(cardId);
            card.Load(new Event[]
            {
                new TaskOpened(cardId, DateTime.UtcNow, "Test"),
                new CardAssigned(card.Id, null, "Jason Mitchell")
            });

            card.Assignee.Should().Be("Jason Mitchell");
        }

        [Fact]
        public void NoChangesAreRaisedWhenAssigneeHasNotChanged()
        {
            var cardId = RandomId();
            var card = new CardAggregate(cardId);
            card.Load(new Event[]
            {
                new TaskOpened(cardId, DateTime.UtcNow, "Test"),
                new CardAssigned(card.Id, null, "Jason Mitchell")
            });

            card.AssignTo("Jason Mitchell");
            
            var changes = card.GetChanges();
            changes.Should().BeEmpty();
        }

        [Fact]
        public void DevelopmentIsStarted()
        {
            var cardId = RandomId();
            var card = new CardAggregate(cardId);
            card.Load(new Event[]
            {
                new TaskOpened(cardId, DateTime.UtcNow, "Test"),
                new CardAssigned(card.Id, null, "Jason Mitchell")
            });

            card.StartDevelopment();

            var changes = card.GetChanges();
            
            changes.Should().BeEquivalentTo(new[]
            {
                new DevelopmentStarted(card.Id, DateTime.UtcNow),
            }, opt => opt.Excluding(x => x.DateStarted).WithStrictOrdering());
        }
        
        [Fact]
        public void HandlesDevelopmentStarted()
        {
            var cardId = RandomId();
            var card = new CardAggregate(cardId);
            card.Load(new Event[]
            {
                new TaskOpened(cardId, DateTime.UtcNow, "Test"),
                new CardAssigned(card.Id, null, "Jason Mitchell"),
                new DevelopmentStarted(cardId, DateTime.UtcNow)
            });

            card.Status.Should().Be(CardStatus.InProgress);
        }

        [Fact]
        public void DoesNotAllowDevelopmentToStartWithoutAssignee()
        {
            var cardId = RandomId();
            var card = new CardAggregate(cardId);
            card.Load(new Event[]
            {
                new TaskOpened(cardId, DateTime.UtcNow, "Test")
            });

            Action action = () => card.StartDevelopment();
            action.Should().ThrowExactly<AssigneeIsRequiredException>();
        }
        
        [Fact]
        public void NoChangesAreRaisedWhenDevelopmentHasAlreadyStarted()
        {
            var cardId = RandomId();
            var card = new CardAggregate(cardId);
            card.Load(new Event[]
            {
                new TaskOpened(cardId, DateTime.UtcNow, "Test"),
                new CardAssigned(card.Id, null, "Jason Mitchell"),
                new DevelopmentStarted(cardId, DateTime.UtcNow)
            });

            card.StartDevelopment();
            
            var changes = card.GetChanges();
            changes.Should().BeEmpty();
        }
    }
}