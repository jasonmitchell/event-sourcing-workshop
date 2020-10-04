# Implementing command handlers


## 01 - Create a command handler to open a new task
As usual we will start with a test for our new command handler. Command handlers typically don't 
contain much business logic so there is little value in testing them directly. Instead we will create
a test to hit multiple layers of the code; we will implement an integration test for our handlers.

Create a new directory called `Commands` inside the `Integration` directory in the `Kanban.Tests` 
project. Next create a file called `OpenTaskTests.cs` inside the new `Commands` directory with the
content below.

```csharp
using Kanban.Domain.Cards;
using Kanban.Domain.Cards.Commands;
using Kanban.Framework;
using Kanban.Tests.Fixtures;
using Xunit;

namespace Kanban.Tests.Integration.Commands
{
    public class OpenTaskTests : IClassFixture<EsdbFixture>
    {
        private readonly EsdbFixture _esdb;
        private readonly Handlers _handlers;

        public OpenTaskTests(EsdbFixture esdb)
        {
            _esdb = esdb;
            
            var eventStore = new EsdbEventStore(esdb.Client);
            var aggregateStore = new CardAggregateStore(eventStore);
            _handlers = new Handlers(aggregateStore);
        }
    }
}
```

Next we will create the `Handlers` class to fix the compiler errors. Create a new directory called 
`Commands` in `Kanban.Domain.Cards`. Next create a file called `Handlers.cs` in the `Commands` directory
with the content below:

```csharp
namespace Kanban.Domain.Cards.Commands
{
    public class Handlers
    {
        private readonly CardAggregateStore _store;

        public Handlers(CardAggregateStore store)
        {
            _store = store;
        }
    }
}
```

This will fix the build errors so now we can implement the test for opening a task. Add the test 
below to `OpenTaskTests.cs`. As usual we will have some compiler errors.

```csharp
[Fact]
public async Task OpensNewCard()
{
    var cardId = Guid.NewGuid().ToString();
    var command = new OpenTask
    {
        Id = cardId,
        Title = "Test"
    };

    await _handlers.Handle(command);

    var streamName = $"Card-{cardId}";
    await _esdb.AssertStreamExists(streamName);
    await _esdb.AssertEventTypes(streamName, new[]
    {
        typeof(TaskOpened)
    });
}
```

Add a new file to `Kanban.Domain.Cards.Commands` called `OpenTask.cs` with the following content:

```csharp
using Kanban.Framework;

namespace Kanban.Domain.Cards.Commands
{
    public class OpenTask : Command
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
}
```

And update `Handlers.cs` to implement the interface `ICommandHandler<OpenTask>`:

```csharp
using System;
using System.Threading.Tasks;
using Kanban.Framework;

namespace Kanban.Domain.Cards.Commands
{
    public class Handlers : ICommandHandler<OpenTask>
    {
        private readonly CardAggregateStore _store;

        public Handlers(CardAggregateStore store)
        {
            _store = store;
        }
        
        public async Task Handle(OpenTask command)
        {
            throw new NotImplementedException();
        }
    }
}
```

Now we have a test which compiles but it won't pass until the command handler has been implemented. 
As mentioned previously there is very little business logic to command handlers. Update the `Handle`
method as follows:

```csharp
public async Task Handle(OpenTask command)
{
    var card = new CardAggregate(command.Id);
    card.OpenTask(command.Title);

    await _store.Save(card);
}
```

The test will now pass and our domain model is nicely abstracted away behind the command handlers.


## 02 - Create a command handler to assign a card to a person
Following the pattern from the previous steps write an integration test for a command handler which
assigns a card to a person and then implement a command called `AssignCard`. Next create a handler for
the command which will load an existing aggregate using the `CardAggregateStore`, call the `AssignTo` 
method on `CardAggregate`, and save the aggregate again.


## 03 - Create a command handler to start development on a card
Following the pattern from the previous steps write an integration test for a command handler which
assigns a card to a person and then implement a command called `StartDevelopment`. Next create a handler for
the command which will load an existing aggregate using the `CardAggregateStore`, call the `StartDevelopment` 
method on `CardAggregate`, and save the aggregate again.