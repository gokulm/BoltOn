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

Implementation Sample
---------------------
The best way to understand the implementation is by looking into [BoltOn.Samples.WebApi](https://github.com/gokulm/BoltOn/tree/master/samples/BoltOn.Samples.WebApi) project's `StudentsController` and by going thru GET, POST and PUT student endpoints, corresponding requests and their handlers. 

Though separate databases could be used for commands and queries i.e., writes and reads respectively, in this sample we have used only one database (which could change when repositories for other databases are implemented) but different tables. 

* Two entities `Student` and `StudentFlattened` are created by inheriting `BaseCqrsEntity`. 
* Student entity is saved in Student table with foreign-key constraint to StudentType table. Commands (aka writes) go to this table.
* StudentFlattened entity is saved in StudentFlattened table, which is denormalized without any foreign-key constraints. Queries (aka reads) go to this table.
* Private and internal constructors are added to both the entities. The private constructor is to support EF and the internal constructor is to allow instantiation of the entity with appropriate request object as parameter.
* Student's internal ctor is called from `CreateStudentHandler`, which gets invoked by `Mediator` from StudentController's POST call.
* `StudentCreatedEvent` event is created by inheriting `CqrsEvent`.  Other properties that are required to create StudentFlattened entity are added. As StudentFlattened is denormalized, only StudentType is added to it and not the StudentTypeId.
* `StudentCreatedEvent` event is triggered in the ctor by calling the base class' `RaiseEvent` method. The RaiseEvent method takes care of populating other properties like Id, SourceId, SourceTypeName and CreatedDate. 

**Note:** Id property will be initialized only if we don't initialize it, whereas all the other properties listed above will be overridden by the framework. The triggered event gets marked for processing by setting the CreatedDate property to null, and it gets added to EventsToBeProcessed property only if it's not already present. 

Here is the internal ctor of Student entity:

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
				StudentType = studentType
			});
		}
	}

* `IRepository<Student>` injected in the `CreateStudentHandler` is registered to use `CqrsRepository<Student>`. Please look into the RegistrationTask class in the BoltOn.Samples.WebApi project for all the other registrations.
* When `AddAsync` of the repository is called in the handler, the repository adds the entity and on while saving changes using the `SaveChanges` method, the events marked for processing are added to a request scoped object called `EventBag`, right after the CreatedDate property is initialized.

**Note:** 

* If CQRS is enabled in the Startup's BoltOn method, [`CqrsInterceptor`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/CqrsInterceptor.cs) is added to the `Mediator` pipeline. 
* The intercepor calls `EventDispatcher` to dispatch events that need to be processed, which inturn publishes events using `IBus`. 
* Even if the dispatcher or the bus fails, the events to be processed will be persisted along with the entity, as the `CqrsIntercepor` is after the `UnitOfWorkIntercepor`, which takes care of committing the transaction. 
* The events that get persisted with the entity but not dispatched will be processed the next time the entity is updated. 
* If there are more than one event to be processed and if one or more fails, it will still dispatch the other events. 
* A MassTransit consumer is registered to handle `StudentCreatedEvent` in the BoltOn.Samples.Console project's RegistrationTask class, and the consumer inturn calls `StudentCreatedEventHandler` 

Like this:

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
* `StudentCreatedEvent` event is processed in the ctor by calling the base class' `ProcessEvent` method. The action delegate passed as a parameter to the method is invoked only if the event is not already processed. After invoking the action delegate, the DestinationTypeName property is populated and the event is added to the ProcessedEvents collection. 
* The ProcessedEvents get persisted along with the entity and thus the collection prevents events getting re-processed. 

Here is the internal ctor of StudentFlattened entity:

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
            });
        }
	}

* `IRepository<StudentFlattened>` injected in the `StudentCreatedEventHandler` is registered to use `CqrsRepository<StudentFlattened>`. 
* When `AddAsync` of the repository is called in the handler, the repository adds the entity and on while saving changes using the `SaveChanges` method, the processed events' ProcessedDate is populated and persisted.

CQRS is implemented even for Student update functionality, so follow the PUT in StudentsController, and UpdateStudentHandler and StudentUpdatedEventHandler handlers.