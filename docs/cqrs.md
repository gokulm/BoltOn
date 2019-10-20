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
* Create your domain entity class by inheriting [`BaseCqrsEntity`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/BaseCqrsEntity.cs), and which will force your entity's Id property to be of type Guid. 
* Create EF mapping configuration class by inheriting [`BaseCqrsEntityMapping`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/BaseCqrsEntityMapping.cs). This takes care of serializing and converting string to bytes while saving, and converting bytes to string deserializing while retrieving. This is done using EF's [Value Conversions](https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions).
* Events get triggered from `RaiseEvent<TEvent>(TEvent @event)` method in the `BaseCqrsEntity` and they get processed in `ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action)`.
* Events must inherit `CqrsEvent`, which implements `ICqrsEvent`, and which inturn implements Mediator's `IRequest`, and thus the events can be handled using `Mediator`.
* Create your request and handlers, and then use the `Mediator` to process your request. Please refer to [Mediator](../mediator) documentation to create handlers.
* Repositories should inherit `CqrsRepository` present in the Data package. In the case of EF, it's [this class](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/CqrsRepository.cs).

Samples
-------
The best way to understand the implementation is by looking into [BoltOn.Samples.WebApi](https://github.com/gokulm/BoltOn/tree/master/samples/BoltOn.Samples.WebApi) project's StudentsController and by going thru GET, POST and PUT student endpoints, corresponding requests and their handlers. 

Though separate databases could be used for commands and queries i.e., writes and reads respectively, in this sample we have used only one database (which could change when repositories for other databases are implemented) but different tables. 

* Two entities `Student` and `StudentFlattened` are created by inheriting `BaseCqrsEntity`. 
* Student entity is saved in Student table with foreign-key constraint to StudentType table. Commands (aka writes) go to this table.
* StudentFlattened entity is saved in StudentFlattened table, which is denormalized without any foreign-key constraints. Queries (aka reads) go to this table.
* Private and internal constructors are added to both the entities. The private constructor is to support EF and the internal constructor is to allow instantiation of the entities with appropriate request objects as parameters.
* Student's internal ctor is called from `CreateStudentHandler`, which gets invoked by `Mediator` from StudentController's POST call.
* `StudentCreatedEvent` event is created by inheriting `CqrsEvent`.  Other properties that are required to create StudentFlattened entity are added. As StudentFlattened is denormalized, only StudentType is added to it and not the StudentTypeId.
* `StudentCreatedEvent` event is triggered in the ctor by calling the base class' `RaiseEvent` method. The RaiseEvent method takes care of populating other properties like Id, SourceId, SourceTypeName and CreatedDate. 

**Note:** Id property will be initialized only if you don't initialize it, whereas all the other properties listed above will be overridden by the framework. The triggered event gets marked for processing by setting the CreatedDate property to null, and it gets added to EventsToBeProcessed property only if it's not already present. 

Here are the internal ctors of Student and StudentFlattened entities:

    internal Student(CreateStudentRequest request, string studentType)
    {
        Id = Guid.NewGuid();
        FirstName = request.FirstName;
        LastName = request.LastName;
        StudentTypeId = request.StudentTypeId;

        RaiseEvent(new StudentCreatedEvent
        {
            StudentId = Id,
            FirstName = FirstName,
            LastName = LastName,
            StudentType = studentType
        });
    }

    internal StudentFlattened(StudentCreatedEvent @event)
    {
        ProcessEvent(@event, e =>
        {
            Id = e.StudentId;
            FirstName = e.FirstName;
            LastName = e.LastName;
            StudentType = e.StudentType;
        });
    }

* `IRepository<Student>` injected in the `CreateStudentHandler` is registered to use `CqrsRepository<Student>`. Please look into the RegistrationTask class in the BoltOn.Samples.WebApi project.
* When `AddAsync` of the repository is called in the handler, the repository adds the entity and on while saving changes using the `SaveChanges` method, the events marked for processing are added to a request scoped object called `EventBag`, right after the CreatedDate property is initialized.