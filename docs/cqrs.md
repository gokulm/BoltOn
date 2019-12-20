To understand more about the Command Query Responsibility Segregation (CQRS) pattern and when to use it, please go over [this post](https://martinfowler.com/bliki/CQRS.html). Most of the CQRS implementations found on the internet mention only about separating the command and the query data stores, but don't mention anything about keeping those two data stores in sync, which is the challenging part, BoltOn's CQRS implementation covers it. The core implementation was majorly inspired by [these series of posts](https://jimmybogard.com/life-beyond-transactions-implementation-primer/).

In order to implement CQRS, you need to do the following:

* Install **BoltOn.Data.EF** or **BoltOn.Data.CosmosDb** NuGet package depending on your read/write data store.
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

* Configure EF DbContext (if you're using SQL) and MassTransit Bus.
* Create your domain entity class and inherit [`BaseCqrsEntity`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/BaseCqrsEntity.cs), which will force your entity's Id property to be of type Guid. 
* Create EF mapping configuration class by inheriting [`BaseCqrsEntityMapping`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/BaseCqrsEntityMapping.cs). This takes care of serializing/deserializing EventsToBeProcessed and ProcessedEvents collections. This is done using EF's [Value Conversions](https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions).
* Events get triggered from `RaiseEvent<TEvent>(TEvent @event)` method in the `BaseCqrsEntity` and they get processed in `ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action)`.
* Create your events and inherit `CqrsEvent`, which implements `ICqrsEvent`, and which inturn implements Mediator's `IRequest`, and thus the events can be handled using `Mediator`.
* Create your request and handlers, and then use the `Mediator` to process your request. Please refer to [Mediator](../mediator) documentation to create handlers.
* Register `IRepository<TEntity>` to [EF Repository](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/Repository.cs) or [CosmosDb Repository](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.CosmosDb/Repository.cs).

How does it work?
-----------------
The best way to understand the implementation is by looking into [BoltOn.Samples.WebApi](https://github.com/gokulm/BoltOn/tree/master/samples/BoltOn.Samples.WebApi) project's `StudentsController` and by going thru GET, POST and PUT student endpoints, corresponding requests and their handlers. 

Though separate databases could be used for commands and queries i.e., writes and reads respectively, in this sample we have used only one database but different tables Student and StudentFlattened. You can switch the read/write store to SQL or CosmosDb (or any other database that will be supported in the future) using `IRepository` registration.

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
* The intercepor calls [EventDispatcher](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/EventDispatcher.cs) to dispatch events that need to be processed, which inturn publishes events using `IBus`. 
* Even if the dispatcher or the bus fails, the events to be processed will be persisted along with the entity, as the `CqrsIntercepor` is after the `UnitOfWorkIntercepor`, which takes care of committing the transaction.
* If there are more than one event to be processed and if one fails, all the subsequent events dispatching gets aborted, so that the order of the events could be maintained.
* The MassTransit consumer registered to handle `StudentCreatedEvent` in the BoltOn.Samples.Console project's RegistrationTask class handles the event using `StudentCreatedEventHandler` 

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

* Over a period of time, **ProcessedEvents** collection could bloat the read entity, so it's better to write an utility to clear them periodically.