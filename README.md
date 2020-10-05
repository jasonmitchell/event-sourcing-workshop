# Event Sourcing with EventStoreDB
This workshop will take you through the basics of building an event sourced application with EventStoreDB.


## Prerequisites
* Docker
* .NET Core 3.1 SDK
* A C# IDE such as VS Code, Visual Studio or Jetbrains Rider


### Starting the infrastructure
The workshop material includes a `docker-compose.yml` which can be used to start an instance of 
EventStoreDB for use in the workshop exercises. To start the infrastructure, run the following 
command from the root of this repository:

```
docker-compose up -d
```

When this command finishes you will be able to access the EventStoreDB dashboard at http://localhost:2113.
To stop EventStoreDB and clean up all data run the following command:

```
docker-compose down
```


## Structure
The workshop exercises are located in the `exercises` directory. There are several stages to the workshop:

* **01:** Implementing an event sourced aggregate
* **02:** Persisting events with EventStoreDB
* **03:** Accessing our domain model using commands
* **04:** Implementing a read model

Each section contains a `start` and `completed` folder and a readme. The readme outlines the steps we will
follow for the exercise. The `start` folder contains the starting point for each exercise and the `completed` 
folder contains what your code could be like by the end of the exercise. Use the `completed` solution if 
you get stuck but I encourage you to work through the exercises without copying code out of the `completed` 
section if possible.

During each exercise we will write tests to accompany our solution. These tests are written using xUnit
and Fluent Assertions.

The repository also includes a completed sample which adds an HTTP API around the domain model using
ASP.NET Core.


## Resources
* [Event Store website](https://eventstore.com/)
* [Event Store documentation](https://developers.eventstore.com/)
* [xUnit](https://xunit.net)
* [Fluent Assertions](https://fluentassertions.com)