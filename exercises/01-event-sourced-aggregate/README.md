# Implementing an Event Sourced Aggregate

## 01 - Create the aggregate type
Create a new directory called `Cards` within the `Kanban.Domain` project; this namespace will contain
all of the code relating to our new aggregate.  Within this directory create a new file called
`CardAggregate.cs` with the following content:

```csharp
using Kanban.Framework;

namespace Kanban.Domain.Cards
{
    public class CardAggregate : EventSourcedAggregateRoot
    {
        public CardAggregate(string id) 
            : base(id) { }

        protected override void Apply(Event @event)
        {
            
        }
    }
}
```

This new class extends `EventSourcedAggregateRoot` which is defined in the `Kanban.Framework` project and
defines the basic logic which is common across aggregates.

Next, create a corresponding test file in the `Kanban.Tests` project with the path `Domain/CardAggregateTests.cs`.


## 02 - Raise an event
In this step we will implement a method in `CardAggregate` which will raise a new event. First, we will create
a test in `CardAggregateTests.cs` for the expected behaviour. In general tests for event sourced aggregate
behaviour follows a typical "Arrange, Act, Assert" (or Given, When, Then) pattern as seen below:

```csharp
[Fact]
public void TestMethod()
{
    // ARRANGE
    // Create an aggregate
    // Replay aggregate history to get the up-to-date state

    // ACT
    // Execute methods on the aggregate

    // ASSERT
    // Extract the changes from the aggregate
    // Assert the changes match the expectation
}
```

Create the following test in `CardAggregateTests.cs` (ignore the compiler errors for now):


```csharp
[Fact]
public void TaskIsOpened()
{
    var card = new CardAggregate(Guid.NewGuid().ToString());
    card.OpenTask("Test task");

    var changes = card.GetChanges();
    changes.Should().BeEquivalentTo(new[]
    {
        new TaskOpened(card.Id, DateTime.UtcNow, "Test task"),
    }, opt => opt.Excluding(x => x.DateOpened).WithStrictOrdering());
}
```

> The method call `changes.Should().BeEquivalentTo(...)` is an assertion using the FluentAssertions 
library which allows us to easily assert that collections match the expectation.

This test defines our first aggregate method `OpenTask`, and our first event `TaskOpened`. Also notice 
that the id of `CardAggregate` is a new `Guid` converted to a string; it is relatively common to use
a type such as `Guid` for aggregate ids as it is often difficult to use incremental ids found in 
relational databases. However always consider if there is a more natural id available for an aggregate
such as an email address for a user.

Create the `TaskOpened` event in the file `Cards/Events/TaskOpened.cs` in the `Kanban.Domain` project:

```csharp
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
```

> Note that the properties in this class are defined as readonly and are set in the constructor. This
makes our event immutable which is considered good practice.

Lastly implement the `OpenTask` method in `CardAggregate`:

```csharp
public void OpenTask(string title)
{
    Ensure.NotNullOrEmpty(nameof(title), title);
    Raise(new TaskOpened(Id, DateTime.UtcNow, title));
}
```

> The call to `Ensure.NotNullOrEmpty(...)` is known as a "guard clause" and isn't specific to event
sourcing or Domain-Driven Design. It allows us to easily ensure that parameters meet our expectations
and exit early if they don't.

The test should now pass.


##  03 - Apply an event to rebuild state
Now that we have an event being raised by our aggregate we can use it to rebuild the state using the
empty `CardAggregate.Apply(...)` method we defined earlier. Again we will create a new test for applying
an event first. These tests generally follow the pattern below:

```csharp
[Fact]
public void TestMethod()
{
    // ARRANGE
    // Create an aggregate

    // ACT
    // Load the aggregate from a set of events

    // ASSERT
    // Assert the state of the aggregate matches the expectation
}
```

Create the following test in `CardAggregateTests.cs` (again ignoring compiler errors):

```csharp
[Fact]
public void HandlesTaskOpened()
{
    var cardId = Guid.NewGuid().ToString();
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
```

We can see from the assertions that the we are populating four properties.  Add these properties to 
`CardAggregate`:

```csharp
public CardType Type { get; private set; }
public CardStatus Status { get; private set; } = CardStatus.ToDo;
public DateTime DateOpened { get; private set; }
public string Title { get; private set; }
```

And also create the enums for `CardType` and `CardStatus` in `Kanban.Domain.Cards`:

```csharp
public enum CardStatus
{
    ToDo,
    InProgress,
    InTesting,
    Done
}

public enum CardType
{
    Task,
    Story,
    Defect
}
```

This will resolve the compiler errors but the test won't pass until the `Apply` method is updated 
to handle the `TaskOpened` event.

```csharp
protected override void Apply(Event @event)
{
    switch (@event)
    {
        case TaskOpened e:
            Type = CardType.Task;
            Title = e.Title;
            DateOpened = e.DateOpened;
            break;
    }
}
```

The test should now pass.


## 04 - Add more events
Continue applying the patterns learned so far to add more operations to `CardAggregate` (don't forget
about tests). Add support for the following requirements (if you get stuck look at the `completed` section
for this exercise):

- Cards should be able to be assigned to a single person.
- It should be possible to start development on a card. Your team follows best practices so cards must
be assigned to someone before development starts (because that's what heroes do!).

> There are many ways to handle the domain validation error when we try to start development without the
card first being assigned to someone. The simplest approach is to create a new exception type and `throw`
an instance of it when the business rules have been violated.


### Additional considerations
What should happen when you try to assign a card to the same person twice in a row? Or starting development
on a card which has already started? Ideally operations should be idempotent meaning that performing the
operation multiple times has the same effect. This means that we shouldn't raise the same event twice in 
a row. Consider writing a test for each of your operations which should be idempotent and then updating 
the aggregate to make them pass.

If you have enough time in this exercise continue extending the operations to model more of a software
development process such as moving cards into testing or raising a new defect.