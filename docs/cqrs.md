The Command Query Responsibility Segregation (CQRS) implementation in this framework was majorly inspired by [these series of posts](https://jimmybogard.com/life-beyond-transactions-implementation-primer/). To understand more about the CQRS pattern and when to use it, please go over [this post](https://martinfowler.com/bliki/CQRS.html).

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
* Create your domain entity class and inherit [`BaseCqrsEntity`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/BaseCqrsEntity.cs), and which will force your entity's Id property to be of type Guid. 
* Events get triggered from `RaiseEvent<TEvent>(TEvent @event)` method in the `BaseCqrsEntity` and they get processed in `ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action)`.
* Events must inherit `CqrsEvent`, which implements `ICqrsEvent`, and which inturn implements `IRequest`, and thus the events can be handled using `Mediator`.
* Create your request and handlers, and then use the `Mediator` to process your request. Please refer to [Mediator](../mediator) documentation to create handlers.

The best way to understand the implementation is by looking into [BoltOn.Samples.WebApi](https://github.com/gokulm/BoltOn/tree/master/samples/BoltOn.Samples.WebApi) project's StudentsController and by going thru GET, POST and PUT student endpoints, corresponding requests and their handlers. `Student` and `StudentFlattened` entities inherit `BaseCqrsEntity`. Events get triggered on create and update of Student entity, and they get handled by handlers registered in [BoltOn.Samples.Console](https://github.com/gokulm/BoltOn/tree/master/samples/BoltOn.Samples.Console) project. 

Though separate databases could be used for commands and queries i.e., writes and reads respectively, in this sample we have used only one database (which could change when repositories for other databases are implemented) but different tables; the Student entity is saved in Student table with foreign-key constraint to StudentType table, whereas the StudentFlattened entity is saved in StudentFlattened table, which is denormalized without any foreign-key constraints.

Student is instantiated in `CreateStudentHandler` using the internal constructor (private ctor is to support EF and the internal ctor is to support unit testing but still encapsulate the properties). StudentCreatedEvent event is raised in the ctor using the BaseCqrsEntity's `RaiseEvent` method. It populates some of the event properties like Id, SourceId, SourceTypeName and CreatedDate. **Note: ** The events get marked to be processed by setting the CreatedDate property to null.