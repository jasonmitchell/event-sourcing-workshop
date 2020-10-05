# Implementing a read model


## 01 - Defining our first projection event handler
By this point you know the drill; we will start with writing a test first. Since projections and 
read models are all about data access, the best approach is normally to write an integration test.
Start by creating a new directory called `Queries` inside the `Integration` directory in the
`Kanban.Tests` project. Inside the new directory create a new file called `CardActivityTests.cs` with
the content below:

```csharp
using System;
using Kanban.Domain.Cards.Queries;
using Kanban.Domain.Cards.Queries.Projections;
using Kanban.Framework;
using Kanban.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kanban.Tests.Integration.Queries
{
    public class CardActivityTests : IClassFixture<EsdbFixture>, IDisposable
    {
        private readonly EsdbFixture _esdb;
        private readonly ICardActivityRepository _repository;
        private readonly ProjectionRunner _runner;
        
        public CardActivityTests(EsdbFixture esdb)
        {
            _esdb = esdb;
            _repository = new InMemoryCardActivityRepository();
            _runner = new ProjectionRunner(esdb.Client, new NullLogger<ProjectionRunner>(), new Projection[]
            {
                new CardActivityProjection(_repository)
            });
        }

        public void Dispose()
        {
            _runner?.Dispose();
        }
    }
}
```

As usual the code won't compile just yet. Before continuing take a moment to review the code of 
`ProjectionRunner` in `Kanban.Framework`. We will revisit this class later in the exercise but take 
note of the `NotImplementedException` for now.

First we will create the `ICardActivityRepository` interface. This will define how we access our 
read model.  Create a new directory called `Queries` in `Kanban.Domain.Cards`

```csharp
using System.Threading.Tasks;

namespace Kanban.Domain.Cards.Queries
{
    public interface ICardActivityRepository
    {
        Task AppendActivityToCard(string cardId, string activity);
        Task<string[]> GetActivityForCard(string cardId);
    }
}
```

The method `AppendActivityToCard` is what we will use to update the read model and `GetActivityForCard`
is how we will query the data. For this exercise we will use an in-memory repository to keep it simple,
in reality this is likely going to be another database such as Postgres or SQL Server.

Create the following file called `InMemoryCardActivityRepository.cs` in `Kanban.Tests.Integration.TestHelpers`.

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Kanban.Domain.Cards.Queries;

namespace Kanban.Tests.Integration.TestHelpers
{
    public class InMemoryCardActivityRepository : ICardActivityRepository
    {
        private readonly Dictionary<string, List<string>> _activityStreams = new Dictionary<string, List<string>>();
        
        public Task AppendActivityToCard(string cardId, string activity)
        {
            if (!_activityStreams.TryGetValue(cardId, out var activityStream))
            {
                activityStream = new List<string>();
                _activityStreams[cardId] = activityStream;
            }
            
            activityStream.Add(activity);
            return Task.CompletedTask;
        }

        public Task<string[]> GetActivityForCard(string cardId)
        {
            if (_activityStreams.ContainsKey(cardId))
            {
                var activityStream = _activityStreams[cardId].ToArray();
                return Task.FromResult(activityStream);
            }

            return Task.FromResult<string[]>(null);
        }
    }
}
```

Lastly we can fix the remaining error by creating an empty projection. Create a file called
`CardActivityProjection` in `Kanban.Domain.Cards.Queries` with the content below.

```csharp
using Kanban.Domain.Cards.Events;
using Kanban.Framework;

namespace Kanban.Domain.Cards.Queries.Projections
{
    public class CardActivityProjection : Projection
    {
        public CardActivityProjection(ICardActivityRepository repository)
        {
            
        }
    }
}
```

This class extends `Projection` which is defined in the `Kanban.Framework` project. `Projection` 
provides methods for defining event handlers and executing them when an event is received.

Now that our code compiles again we can write our first test. Testing projections can be a little bit
tricky because event handlers are not executed immediately so our tests need to be able to handle a
delay. The `Wait` class provided in `Kanban.Tests.Integration.TestHelpers` provides some helper 
methods to deal with this delay safely.

The process for testing projections is outlined below:

```csharp
public void TestMethod()
{
    // ARRANGE
    // Seed the streams in EventStoreDB with example events

    // ACT
    // Start the projection runner to begin the subscription

    // ASSERT
    // Wait until the read model is generated as expected
}
```

Add the following test to `CardActivityTests`. This test will check that when the `TaskOpened` event is 
raised that our code generates a description of what happened for an activity stream.

```csharp
[Fact]
public async Task GeneratesActivityStreamForCard()
{
    var cardId = Guid.NewGuid().ToString();
    var streamName = $"Card-{cardId}";

    await _esdb.SeedStream(streamName, new Event[]
    {
        new TaskOpened(cardId, DateTime.UtcNow.AddHours(-1), "Test")
    });

    await _runner.Start(CancellationToken.None);

    await Wait.UntilAsserted(async () =>
    {
        var activity = await _repository.GetActivityForCard(cardId);
        activity.Should().BeEquivalentTo(new[]
        {
            "Task 'Test' was opened"
        }, opt => opt.WithStrictOrdering());
    });
}
```

Now we can finally write the first event handler for our projection. Update the constructor of
`CardActivityProjection` to match the following code. This will set up a handler for the `TaskOpened`
event and call `AppendActivityToCard` on our repository to update the read model.

```csharp
public CardActivityProjection(ICardActivityRepository repository)
{
    When<TaskOpened>(async e =>
    {
        await repository.AppendActivityToCard(e.CardId, $"Task '{e.Title}' was opened");
    });
}
```

If you run the tests now they will fail because of the `NotImplementedException` highlighted earlier 
in `ProjectionRunner`. In the next step we will update `ProjectionRunner` to subscribe to event store
and deliver events to our projection.


## 02 - Subscribe to event store
Using the documentation available at 
https://developers.eventstore.com/clients/dotnet/generated/v20.6.0/subscribing-to-streams/basics.html, 
update the `Start` method in `ProjectionRunner` to start a new subscription to the `$all` stream in
EventStoreDB. For this exercise just create a subscription from the beginning of the stream and make
sure you filter out any system events in your subscription.

> In production you need to be able to restart projections from a specific location so our application
doesn't always have to read the full stream. To do this you need to periodically record the position
which events have been handled upto. 

Once you have implemented the subscription in the `ProjectionRunner` class the test should pass at
last. If you get stuck check out the completed sample for this exercise.


## 03 - Add more event handlers to the projection
Using the patterns from earlier in the exercise update the `CardActivityProjection` to handle the 
`CardAssigned` and `DevelopmentStarted` events and update the read model to generate friendly 
descriptions of them. Don't forget to update the tests.
