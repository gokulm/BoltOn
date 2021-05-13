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
Most of the CQRS implementations found on the internet mention only about separating the command and the query data stores, but do not mention how to keep the two stores in sync, which is the most challenging part, but BoltOn covers it. **However, the implementation here is just an idea and it doesn't cover all the possible usecases, so feel free to tweak the way you want it.**

**Data store synchronization could be handled by the following ways:**


1. Using a feature like database mirroring (if SQL server), if both the read and writes stores use the same database technology and schemas.
2. By persisting data in the write store and publishing an event to an enterprise bus; updating the read store could be handled by a subscriber to the event. But, this will be consistent only if persisting to the write store and publishing are part of a single transaction. As most of the buses do not support transactions, if write store persistence is successful and publishing to bus fails, the read store would be out of sync. Or, the other way, by publishing event to an enterprise bus and then persisting data in the write store. But, this also relies on transaction, else write store could be out of sync. BoltOn overcomes it using [outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html).
3. Event sourcing - there are many libraries supporting event sourcing with CQRS.

BoltOn synchronizes data using pub/sub, but without using transactions, it's a slight variation of method 2 mentioned above, basically by doing something like this:

    public void PlaceOrder(Order order)
    {
        BeginTransaction();
        Try 
        {
            SaveOrderToDataBase(order);
            ev = new OrderPlaced(Order);
            SaveEventToDataBase(ev);
            CommitTransaction();
        }
        Catch 
        {
            RollbackTransaction();
            return;
        }

        PublishEventAsync(ev);    
    }

    async Task PublishEventAsync(BussinesEvent ev) 
    {
        BegintTransaction();
        try 
        {
            await DeleteEventAsync(ev);
            await bus.PublishAsync(ev);
            CommitTransaction();
        }
        catch 
        {
            RollbackTransaction();
        }
    }

 **Note:** Code block copied from [this StackOverflow answer](https://stackoverflow.com/questions/30780979/best-way-to-ensure-an-event-is-eventually-published-to-a-message-queuing-sytem). 
 
 In BoltOn, business entity is persisted along with the events raised in the same data store **as part of a collection within the entity**, and then the persisted events get published to the bus. As events are persisted along with the entity, even if the publish fails, events could be republished later on, provided the business is fine with [eventual consistency](https://en.wikipedia.org/wiki/Eventual_consistency). 

In order to implement CQRS using BoltOn, you need to do the following:

* Install **BoltOn.Data.EF** NuGet package depending on your read/write data store (currently CQRS is supported only for SQL using EF).
* Install **BoltOn.Bus.MassTransit** NuGet package. Refer to [Data](../data) and [Bus](../bus) documentation to enable the corresponding modules.
* Configure EF DbContext (if you're using SQL) and MassTransit Bus.
* Create your domain entity class and inherit [`BaseDomainEntity`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/BaseDomainEntity.cs) and override DomainEntityId property to return your Entity's Id property as string. 
* Create EF mapping configuration class by inheriting [`BaseDomainEntityMapping`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/BaseDomainEntityMapping.cs). This is mainly to ignore EventsToBeProcessed and PurgeEvents propery mappings. *Events that get triggered using `RaiseEvent<TEvent>(TEvent @event)` method in the `BaseDomainEntity` get added to EventsToBeProcessed collection and they get dispatched in the repository.*
* Create your events by implementing `IDomainEvent` or by inheriting [`BaseDomainEvent`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/BaseDomainEvent.cs), and which inturn implements Requestor's `IRequest`, and thus the events can be handled using `Requestor`.
* Create your request and handlers, and then use the `Requestor` to process your request. Please refer to [Requestor](../requestor) documentation to create handlers.
* Register `IRepository<TEntity>` to EF [`CqrsRepository`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Data.EF/CqrsRepository.cs).
* Create a table for EventStore. Here is the script:

        CREATE TABLE [dbo].[EventStore](
        [EventId] [uniqueidentifier] NOT NULL,
        [Data] [nvarchar](max) NULL,
        [EntityType] [nvarchar](max) NULL,
        [EntityId] [nvarchar](max) NULL,
        [CreatedDate] [datetimeoffset](7) NULL,
        [ProcessedDate] [datetimeoffset](7) NULL
        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
        GO
        ALTER TABLE [dbo].[EventStore] ADD  CONSTRAINT [PK_EventStore] PRIMARY KEY CLUSTERED 
        (
            [EventId] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
        GO


* Create EF mapping for [`EventStore`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Cqrs/EventStore.cs), add the mapping to your DbContext and register `IRepository<EventStore>`.

All the registration:

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDefaultCorrelationId();
        services.BoltOn(options =>
        {
            options.BoltOnEFModule();
            options.BoltOnMassTransitBusModule();
        });

        var boltOnSamplesDbConnectionString = Configuration.GetValue<string>("BoltOnSamplesDbConnectionString");

        services.AddDbContext<SchoolDbContext>(options =>
        {
            options.UseSqlServer(boltOnSamplesDbConnectionString);
        });

        var rabbitmqUri = Configuration.GetValue<string>("RabbitMqUri");
        var rabbitmqUsername = Configuration.GetValue<string>("RabbitMqUsername");
        var rabbitmqPassword = Configuration.GetValue<string>("RabbitMqPassword");
        var redisUrl = Configuration.GetValue<string>("RedisUrl");
        services.AddMassTransit(x =>
        {
            x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(rabbitmqUri), hostConfigurator =>
                {
                    hostConfigurator.Username(rabbitmqUsername);
                    hostConfigurator.Password(rabbitmqPassword);
                });
            }));
        });

        services.AddTransient<IRepository<EventStore>, Repository<EventStore, SchoolDbContext>>();
        services.AddTransient<IRepository<Student>, Repository<Student, SchoolDbContext>>();
        services.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, SchoolDbContext>>();
        services.AddTransient<IQueryRepository<StudentType>, QueryRepository<StudentType, SchoolDbContext>>();
        services.AddTransient<IQueryRepository<Course>, QueryRepository<Course, SchoolDbContext>>();
    }

How does it work?
-----------------
The best way to understand the implementation is by looking into [BoltOn.Samples.WebApi](https://github.com/gokulm/BoltOn/tree/master/samples/BoltOn.Samples.WebApi) project's `StudentsController` and by going thru GET, POST and PUT student endpoints, corresponding requests and their handlers. 

In this sample we have used only two tables - Student and StudentFlattened.

* The events that get raised from your entities (that inherit BaseDomainEntity) get added to EventsToBeProcessed collection. Two entities [`Student`](https://github.com/gokulm/BoltOn/blob/master/samples/BoltOn.Samples.Application/Entities/Student.cs) and [`StudentFlattened`](https://github.com/gokulm/BoltOn/blob/master/samples/BoltOn.Samples.Application/Entities/StudentFlattened.cs) inherit `BaseDomainEntity`. Student entity is saved in Student table with foreign-key constraint to StudentType table. Commands (aka writes) go to this table. StudentFlattened entity is saved in StudentFlattened table, which is denormalized without any foreign-key constraints. Queries (aka reads) go to this table. Private and internal constructors are added to both the entities. The private constructor is to support EF and the internal constructor is to allow instantiation of the entity with appropriate request object as parameter.
* Student's internal ctor is called from `CreateStudentHandler`, which gets invoked by `Requestor` from StudentController's POST call.
* `StudentCreatedEvent` event inherits `BaseDomainEvent`. Other properties that are required to create StudentFlattened entity are added. 
* `StudentCreatedEvent` event is triggered in the ctor by calling the base class' `RaiseEvent` method. The RaiseEvent method takes care of populating EventId if it's not already populated. 

**Note:** The triggered events get added to EventsToBeProcessed collection only if they're not already present. 

Here is the Student entity:

    public class Student : BaseDomainEntity
	{
		private List<StudentCourse> _courses = new List<StudentCourse>();

		public virtual Guid StudentId { get; private set; }
		public virtual string FirstName { get; private set; }
		public virtual string LastName { get; private set; }
		public virtual string Email { get; private set; }
		public virtual int StudentTypeId { get; private set; }

		public override bool PurgeEvents { get; set; } = false;

		public override string DomainEntityId => StudentId.ToString();

		private Student()
		{
		}

		internal Student(CreateStudentRequest request, string studentType)
		{
			StudentId = Guid.NewGuid();
			FirstName = request.FirstName;
			LastName = request.LastName;
			StudentTypeId = request.StudentTypeId;
			Email = request.Email;

			RaiseEvent(new StudentCreatedEvent(StudentId, FirstName, LastName,
				Email, StudentTypeId, studentType));
		}
    }


* `IRepository<Student>` injected in the `CreateStudentHandler` is registered to use `CqrsRepository<Student>`. Please look into the Startup class in the BoltOn.Samples.WebApi project for all the other registrations.
* When `AddAsync` of the repository is called in the handler, the repository adds the entity and  while saving changes, the events get added to EventStore table along with Student entity in the same transaction, and in a separate transaction the events get published to the bus.
* The PurgeEvents property in `BaseDomainEntity` is set to true by default, which controls whether the events in `EventStore` should be deleted or not after publishing the events. 
* If there are more than one event to be processed and if one fails, all the subsequent events dispatching get aborted, so that the order of the events could be maintained.
* The MassTransit consumer registered to handle `StudentCreatedEvent` in the BoltOn.Samples.Console project's Startup class handles the event using `StudentCreatedEventHandler`.

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

* StudentFlattened's internal ctor is called from `StudentCreatedHandler`, which gets invoked by `Requestor` from `BoltOnMassTransitConsumer<StudentCreatedEvent>`

Here is the StudentFlattened entity:

    public class StudentFlattened 
	{
		public Guid StudentId { get; set; }
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string StudentType { get; private set; }
		public int StudentTypeId { get; private set; }

		private StudentFlattened()
		{
		}

		internal StudentFlattened(StudentCreatedEvent @event)
		{
			StudentId = @event.StudentId;
			FirstName = @event.FirstName;
			LastName = @event.LastName;
			StudentType = @event.StudentType;
			StudentTypeId = @event.StudentTypeId;
		}
	}

**Note:**

* By default the events to be processed get purged right after dispatching them, in case if you do not want them to be purged, set CqrsOptions' **PurgeEventsToBeProcessed** property to false while bootstraping the app. 

    Like this:

        var serviceCollection = new ServiceCollection();
        serviceCollection.BoltOn(b =>
        {
            b.BoltOnAssemblies(GetType().Assembly);
            b.BoltOnEFModule();
            b.BoltOnMassTransitBusModule();
            b.BoltOnCqrsModule(o => o.PurgeEventsToBeProcessed = false);
        });

    Purging is done in the `CqrsInterceptor` using a delegate that gets initialized in the `CqrsRepository`.

* The processed events get persisted along with the read entity to mainly maintain *idempotency* i.e., the events that get dispatched more than once due to queue failure or events to processed purging failure may reach read side consumer more than once, so to prevent it, processed events get persisted so that before processing an event the collection can be checked.
*  But, over a period of time, **ProcessedEvents** collection could bloat the read entity, so you could set CqrsOptions' **PurgeEventsProcessedBefore** to a TimeSpan while bootstrapping the application. Say you set it to TimeSpan.FromHours(12), all the events that were persisted before 12 hours will be purged.

    Like this:

        var serviceCollection = new ServiceCollection();
        serviceCollection.BoltOn(b =>
        {
            b.BoltOnAssemblies(GetType().Assembly);
            b.BoltOnEFModule();
            b.BoltOnMassTransitBusModule();
            b.BoltOnCqrsModule(c => c.PurgeEventsProcessedBefore = TimeSpan.FromHours(12));
        });

* In case if the RabbitMq is down, dispatching will fail but EventsToBeProcessed will get persisted along with the entity; the next time when an event gets raised within the same entity, the failed events will be dispached.
