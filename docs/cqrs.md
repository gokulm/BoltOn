Command Query Responsibility Segregation (CQRS)
-----------------------------------------------
The [CQRS](https://martinfowler.com/bliki/CQRS.html) implementation in this framework was majorly inspired by [these series of posts](https://jimmybogard.com/life-beyond-transactions-implementation-primer/). To understand more about the CQRS pattern and when to use it, please go over [this post](https://martinfowler.com/bliki/CQRS.html).

In order to implement CQRS, you need to do the following:

* Install **BoltOn.Data.EF** (soon support for CosmosDb will be added) NuGet package.
* Install **BoltOn.Bus.MassTransit** NuGet package.
* Refer to [Data](../data) and [Bus](../bus) documentation to enable the corresponding modules.
* Enable CQRS by calling BoltOnCqrsModule() in BoltOn() method.

Like this:

    var serviceCollection = new ServiceCollection();
    serviceCollection.BoltOn(b =>
    {
        b.BoltOnAssemblies(GetType().Assembly);
        b.BoltOnEFModule();
        b.BoltOnMassTransitBusModule();
        b.BoltOnCqrsModule();
    });

* Configure EF DbContext and MassTransit Bus.
* Create your domain entity class and inherit [`BaseCqrsEntity`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/BaseCqrsEntity.cs), and which will force your entity's Id property to be of type Guid. The `BaseCqrsEntity` class has two properties `EventsToBeProcessed` and `ProcessedEvents`, and two methods `RaiseEvent<TEvent>(TEvent @event` and `ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action)` that facilitates to implement the pattern. 