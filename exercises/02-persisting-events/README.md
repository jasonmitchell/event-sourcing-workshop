# Persisting Events


## 01 - Create a new aggregate store
As we are about to start interacting with a database we want to write some integration tests to ensure
our code is writing to EventStoreDB correctly. Start by adding a folder called `Integration` to the
`Kanban.Tests` project and then create a file called `CardAggregateStoreTests.cs` inside with the 
following content:

```csharp
using Kanban.Domain.Cards;
using Kanban.Framework;
using Kanban.Tests.Fixtures;
using Xunit;

namespace Kanban.Tests.Integration
{
    public class CardAggregateStoreTests : IClassFixture<EsdbFixture>
    {
        private readonly EsdbFixture _esdb;
        private readonly CardAggregateStore _aggregateStore;
        
        public CardAggregateStoreTests(EsdbFixture esdb)
        {
            _esdb = esdb;
            var eventStore = new EsdbEventStore(esdb.Client);
            _aggregateStore = new CardAggregateStore(eventStore);
        }
    }
}
```

> `EsdbFixture` provides some useful methods for testing code which interacts with EventStoreDB and 
also demonstrates basic usage of the client API for reading and writing events.

> Typically you wouldn't write integration tests as granular as this but for the purpose of the exercise
they will be helpful to have.

Next we will fix the errors so the code compiles. Create a new file called `EsdbEventStore` in the 
`Kanban.Framework` project with the content below:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.Client;

namespace Kanban.Framework
{
    public class EsdbEventStore : IEventStore
    {
        private readonly EventStoreClient _client;

        public EsdbEventStore(EventStoreClient client)
        {
            _client = client;
        }
        
        public async Task<IEnumerable<Event>> ReadStream(string streamName)
        {
            throw new System.NotImplementedException();
        }

        public async Task AppendToStream(string streamName, long expectedVersion, IEnumerable<Event> events, string correlationId)
        {
            throw new System.NotImplementedException();
        }
    }
}
```

Next, reate a new file called `CardAggregateStore.cs` in the `Kanban.Domain.Cards` namespace with the 
following content:

```csharp
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
```

This new aggregate store is how we will load and save aggregates in our code. The base class 
`EventSourcedAggregateStore<T>` is already provided by the `Kanban.Framework` project and
implements the basic saving and loading logic. The implementation detail for integratin EventStoreDB
will be added to `EsdbEventStore`; this abstraction allows our domain project to remain free from
infrastructure concerns.


## 02 - Use the EventStoreDB client API to save an aggregate
First create a test in `CardAggregateStoreTests.cs` which will create a new `CardAggregate`, perform
some operations on it, save it using the `CardAggregateStore`, and lastly assert that the expected 
stream exists and that it contains the expected events. Below is an example test:

```csharp
[Fact]
public async Task SavesCardAggregateChangesToStream()
{
    var cardId = Guid.NewGuid().ToString();
    var card = new CardAggregate(cardId);
    card.OpenTask("Test");
    card.AssignTo("Jason Mitchell");
    card.StartDevelopment();

    var expectedChanges = card.GetChanges().ToArray();
    await _aggregateStore.Save(card);

    var streamName = $"Card-{cardId}";
    await _esdb.AssertStreamExists(streamName);
    await _esdb.AssertEvents(streamName, expectedChanges);
}
```

> This test uses the `EsdbFixture` to perform assertions against streams in EventStoreDB to keep the
tests a little tidier.

If you run the tests now they will fail as the code in `EsdbEventStore` currently throws a 
`NotImplementedException`. Using the documentation available at 
https://developers.eventstore.com/clients/dotnet/generated/v20.6.0/writing-events/basics.html, 
implement the `AppendToStream` method in the `EsdbEventStore` class and get the integration
test to pass.  Use the helper methods in the `EventSerializer` class within the `Kanban.Framework`
project to serialize events to JSON.

Once you get the test passing try to update your implementation to save a `$correlationId` property
to the metadata for each event using the parameter provided to the `AppendToStream` method. If it helps
try to write a test for the metadata first. You can use `Dictionary<string, string>` to define the
metadata before serialization in `EsdbEventStore`.


## 03 - Use the EventStoreDB client API to load an aggregate
As before start with a new integration test. This time the test should pre-populate a stream in 
EventStoreDB, load the aggregate using `CardAggregateStore`, and assert that the aggregate is loaded
and has the expected version. See the following example test:

```csharp
[Fact]
public async Task LoadsCardAggregateFromStream()
{
    var cardId = Guid.NewGuid().ToString();
    var streamName = $"Card-{cardId}";

    await _esdb.SeedStream(streamName, new Event[]
    {
        new TaskOpened(cardId, DateTime.UtcNow.AddDays(-1), "Test task"),
        new CardAssigned(cardId, null, "Jason Mitchell"),
        new DevelopmentStarted(cardId, DateTime.UtcNow)
    });

    var card = await _aggregateStore.Load(cardId);
    card.Version.Should().Be(2);
    
    // Sanity check to ensure state is reconstructed
    card.Title.Should().Be("Test task");
}
```

> Note the sanity check at the bottom. This is to ensure that the logic of `EventSourcedAggregateStore<T>`
actually applies our events to the aggregate to mutate state. Ideally the `Kanban.Framework` itself should be
tested to ensure that this is the case; such sloppiness probably hints at a lazy framework author...

Like before if you run the tests now it will fail due to a `NotImplementedException`. Using the documentation
available at https://developers.eventstore.com/clients/dotnet/generated/v20.6.0/reading-events/basics.html,
implement the `ReadStream` method in the `EsdbEventStore` class and get the integration test to pass.
Again, use the helper methods in the `EventSerializer` class to deserialize events from JSON.
