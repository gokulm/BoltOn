Command Query Responsibility Segregation (CQRS) pattern derives from the principle [Command Query Separation](https://martinfowler.com/bliki/CommandQuerySeparation.html), which states that:

<blockquote>
<p>[E]very method should either be a command that performs an
action, or a query that returns data to the caller, but not both. In other
words, Asking a question should not change the answer.</p>

<p>– <cite><a href="https://en.wikipedia.org/wiki/Command%E2%80%93query_separation">Wikipedia</a></cite></p>
</blockquote>

Command Query Responsibility Separation, or CQRS, takes this principle one step further.

<blockquote>
<p>CQRS is simply the creation of two objects where there was previously only
one. The separation occurs based upon whether the methods are a command or
a query (the same definition that is used by Meyer in Command and Query
Separation: a command is any method that mutates state and a query is any
method that returns a value).</p>

<p>– <cite>Greg Young</cite></p>
</blockquote>

To know more about the CQRS pattern and when to use it, please go over [this post](https://martinfowler.com/bliki/CQRS.html). 

Implementation
--------------
Most of the CQRS implementations found on the internet mention only about separating the command and the query data stores, but do not mention how to keep the two stores in sync, which is the most challenging part, but BoltOn's CQRS implementation covers it. 

**Data store synchronization could be handled by the following ways:**


1. Using a feature like database mirroring (if SQL server), if both the read and writes stores use the same database technology and schemas
2. By persisting data in the write store and publishing an event to an enterprise bus; updating the read store could be handled by a subscriber to the event. But, this will be consistent only if persisting to the write store and publishing are part of a single transaction. As most of the buses do not support transactions, if write store persistence is successful and publishing to bus fails, the read store would be out of sync. Or, the other way, by publishing event to an enterprise bus and then persisting data in the write store. But, this also relies on transaction, else write store could be out of sync.
3. Event sourcing - there are many libraries supporting event sourcing with CQRS.

BoltOn synchronizes data using pub/sub, but without using transactions, it's a slight variation of method 2 mentioned above. The implementation was majorly inspired by [these series of posts](https://jimmybogard.com/life-beyond-transactions-implementation-primer/). In BoltOn, business entity is persisted along with the events raised in the same data store **as part of a collection within the entity**, and then the persisted events get published to the bus. As events are persisted along with the entity, even if the publish fails, events could be republished later on, provided the business is fine with [eventual consistency](https://en.wikipedia.org/wiki/Eventual_consistency). 

In order to implement CQRS using BoltOn, you need to do the following:

* Install **BoltOn.Data.EF** or **BoltOn.Data.CosmosDb** NuGet package depending on your read/write data store.
* Install **BoltOn.Bus.MassTransit** NuGet package. Refer to [Data](../data) and [Bus](../bus) documentation to enable the corresponding modules.
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

* Configure EF DbContext (if you're using SQL) and MassTransit Bus.
* Create your domain entity class and inherit [`BaseCqrsEntity`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/BaseCqrsEntity.cs), which will force your entity's Id property to be of type Guid. 
* Create EF mapping configuration class by inheriting [`BaseCqrsEntityMapping`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/BaseCqrsEntityMapping.cs). This takes care of serializing/deserializing EventsToBeProcessed and ProcessedEvents collections. This is done using EF's [Value Conversions](https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions). Events get triggered from `RaiseEvent<TEvent>(TEvent @event)` method in the `BaseCqrsEntity` and they get processed in `ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action)`.
* Create your events and inherit `CqrsEvent`, which implements `ICqrsEvent`, and which inturn implements Mediator's `IRequest`, and thus the events can be handled using `Mediator`.
* Create your request and handlers, and then use the `Mediator` to process your request. Please refer to [Mediator](../mediator) documentation to create handlers.
* Register `IRepository<TEntity>` to [EF Repository](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/Repository.cs) or [CosmosDb Repository](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.CosmosDb/Repository.cs).

How does it work?
-----------------
The best way to understand the implementation is by looking into [BoltOn.Samples.WebApi](https://github.com/gokulm/BoltOn/tree/master/samples/BoltOn.Samples.WebApi) project's `StudentsController` and by going thru GET, POST and PUT student endpoints, corresponding requests and their handlers. 

Though separate databases could be used for commands and queries i.e., writes and reads respectively, in this sample we have used only one database, but different tables Student and StudentFlattened. You can **switch the read/write store to SQL or CosmosDb** (or any other database that will be supported in the future) by just changing the `IRepository` registration.

* The events that get raised from your entities (that inherit BaseCqrsEntity) get added to EventsToBeProcessed collection. Two entities [`Student`](https://github.com/gokulm/BoltOn/blob/master/samples/BoltOn.Samples.Application/Entities/Student.cs) and [`StudentFlattened`](https://github.com/gokulm/BoltOn/blob/master/samples/BoltOn.Samples.Application/Entities/StudentFlattened.cs) inherit `BaseCqrsEntity`. Student entity is saved in Student table with foreign-key constraint to StudentType table. Commands (aka writes) go to this table. StudentFlattened entity is saved in StudentFlattened table, which is denormalized without any foreign-key constraints. Queries (aka reads) go to this table. Private and internal constructors are added to both the entities. The private constructor is to support EF and the internal constructor is to allow instantiation of the entity with appropriate request object as parameter.
* Student's internal ctor is called from `CreateStudentHandler`, which gets invoked by `Mediator` from StudentController's POST call.
* `StudentCreatedEvent` event inherits `CqrsEvent`. Other properties that are required to create StudentFlattened entity are added. 
* `StudentCreatedEvent` event is triggered in the ctor by calling the base class' `RaiseEvent` method. The RaiseEvent method takes care of populating other properties like Id, SourceId, SourceTypeName and CreatedDate. 

**Note:** Id property will be initialized only if we don't initialize it, whereas all the other properties listed above will be overridden by the framework. The triggered event gets marked for processing by setting the CreatedDate property to null, and it gets added to EventsToBeProcessed property only if it's not already present. 

Here is the Student entity:

    public class Student : BaseCqrsEntity
	{
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public int StudentTypeId { get; private set; }

		private Student()
		{
		}

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
				StudentType = studentType,
				StudentTypeId = StudentTypeId
			});
		}
	}


* `IRepository<Student>` injected in the `CreateStudentHandler` is registered to use `Repository<Student>`. Please look into the RegistrationTask class in the BoltOn.Samples.WebApi project for all the other registrations.
* When `AddAsync` of the repository is called in the handler, the repository adds the entity and on while saving changes, the events marked for processing are added to a request scoped object called `EventBag`.
* If CQRS is enabled in the Startup's BoltOn method, [`CqrsInterceptor`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/CqrsInterceptor.cs) is added to the `Mediator` pipeline. 
* The intercepor calls [EventDispatcher](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/EventDispatcher.cs) to dispatch events that need to be processed, which inturn publishes events using `IBus`. You could write your own implementation of `IEventDispatcher` or `IBus` if the built-in classes do not satisfy your needs.
* Even if the dispatcher or the bus fails, the events to be processed will be persisted along with the entity, as the `CqrsIntercepor` is after the `UnitOfWorkIntercepor`, which takes care of committing the transaction.
* If there are more than one event to be processed and if one fails, all the subsequent events dispatching get aborted, so that the order of the events could be maintained.
* The MassTransit consumer registered to handle `StudentCreatedEvent` in the BoltOn.Samples.Console project's RegistrationTask class handles the event using `StudentCreatedEventHandler`.

Here is the registration:

    container.AddMassTransit(x =>
    {
        x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            var host = cfg.Host(new Uri("rabbitmq://localhost:5672"), hostConfigurator =>
            {
                hostConfigurator.Username("guest");
                hostConfigurator.Password("guest");
            });

            cfg.ReceiveEndpoint("StudentCreatedEvent_queue", ep =>
            {
                ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentCreatedEvent>>());
            });
        }));
    });

* StudentFlattened's internal ctor is called from `StudentCreatedHandler`, which gets invoked by `Mediator` from `BoltOnMassTransitConsumer<StudentCreatedEvent>`
* `StudentCreatedEvent` event is processed in the ctor by calling the base class' `ProcessEvent` method. The action delegate passed as a parameter to the method is invoked only if the event is not already processed. After invoking the action delegate, the DestinationId and the DestinationTypeName properties are populated and the event is added to the ProcessedEvents collection. 
* The ProcessedEvents get persisted along with the entity and thus the collection prevents events getting re-processed. 

Here is the StudentFlattened entity:

    public class StudentFlattened : BaseCqrsEntity
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
		public string StudentType { get; private set; }

		private StudentFlattened()
        {
        }

        internal StudentFlattened(StudentCreatedEvent @event)
        {
            ProcessEvent(@event, e =>
            {
                Id = e.StudentId;
                FirstName = e.FirstName;
                LastName = e.LastName;
				StudentType = e.StudentType;
				StudentTypeId = e.StudentTypeId;
            });
        }
	}

* `IRepository<StudentFlattened>` injected in the `StudentCreatedEventHandler` is registered to use `Repository<StudentFlattened>`. 
* When `AddAsync` of the repository is called in the handler, the repository adds the entity and on while saving changes using the `SaveChanges` method, the processed events' ProcessedDate is populated and persisted.
* If `IRepository<StudentFlattened>` is registered to inject CosmosDb `Repository` and appropriate CosmosDb configurations are added, data can be synced to CosmosDb.

**Note:**

* To purge the events to be processed right after dispatching them, set CqrsOptions' **PurgeEventsToBeProcessed** property to true while bootstraping the app. 

    Like this:

        var serviceCollection = new ServiceCollection();
        serviceCollection.BoltOn(b =>
        {
            b.BoltOnAssemblies(GetType().Assembly);
            b.BoltOnEFModule();
            b.BoltOnMassTransitBusModule();
            b.BoltOnCqrsModule(o => o.PurgeEventsToBeProcessed = true);
        });

    It's handled using [`EventPurger`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/EventPurger.cs). You could write your own implementation of `IEventPurger` if the built-in purger do not satisfy your needs.

* In case if the RabbitMq is down, EventsToBeProcessed will get persisted along with the entity, but dispatching will fail, so it's better to write an utility to go over the write store periodically and dispatch all the unprocessed events in the EventsToBeProcessed collection of every entity. Or, implement some sort of [outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html). 
* Over a period of time, **ProcessedEvents** collection could bloat the read entity, so it's better to write an utility to clear them periodically.