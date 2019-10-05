using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bus.MassTransit;
using BoltOn.Logging;
using Moq;
using BoltOn.Tests.Other;
using System.Linq;
using BoltOn.Bootstrapping;
using MassTransit;
using BoltOn.Mediator.Pipeline;
using BoltOn.Cqrs;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BoltOn.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BoltOn.Tests.Cqrs
{
	[Collection("IntegrationTests")]
	public class CqrsIntegrationTests : IDisposable
	{
		public CqrsIntegrationTests()
		{
			Bootstrapper
				.Instance
				.Dispose();
		}

		[Fact]
		public async Task MediatorHandle_WithCqrs_ReturnsResult()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnEFModule();
				b.EnableCqrs();
				b.BoltOnMassTransitBusModule();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint("TestCqrsUpdatedEvent_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdatedEvent>>());
					});
				}));
			});

			var logger = new Mock<IBoltOnLogger<TestCqrsHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var logger2 = new Mock<IBoltOnLogger<TestCqrsUpdatedEventHandler>>();
			logger2.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger2.Object);

			var logger3 = new Mock<IBoltOnLogger<CqrsInterceptor>>();
			logger3.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger3.Object);

			var logger4 = new Mock<IBoltOnLogger<EventDispatcher>>();
			logger4.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger4.Object);

			var logger5 = new Mock<IBoltOnLogger<ProcessedEventPurger>>();
			logger5.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger5.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			await mediator.ProcessAsync(new TestCqrsRequest { Input = "test" });

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(1000);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsUpdatedEventHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Building repository. SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Removed event. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										"Fetched BaseCqrsEntity. Id: b33cac30-5595-4ada-97dd-f5f7c35c0f4c"));
			var repository = serviceProvider.GetService<IRepository<TestCqrsWriteEntity>>();
			var entity = repository.GetById("b33cac30-5595-4ada-97dd-f5f7c35c0f4c");
			Assert.True(entity.EventsToBeProcessed.Count == 0);
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.Events.Count == 0);
		}

		[Fact]
		public void MediatorHandle_WithCqrsAndNonAsync_ThrowsException()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnEFModule();
				b.EnableCqrs();
				b.BoltOnMassTransitBusModule();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint("TestCqrsUpdatedEvent_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdatedEvent>>());
					});
				}));
			});

			var logger = new Mock<IBoltOnLogger<TestCqrsHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var mediator = serviceProvider.GetService<IMediator>();

			// act 
			var ex = Record.Exception(() => mediator.Process(new TestCqrsRequest { Input = "test" }));

			// assert
			Assert.NotNull(ex);
			Assert.Equal("CQRS not supported for non-async calls", ex.Message);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"{nameof(TestCqrsHandler)} invoked"));
		}

		[Fact]
		public async Task MediatorHandle_WithCqrsAndFailedBus_EventsDoNotGetProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnEFModule();
				b.EnableCqrs();
				b.BoltOnMassTransitBusModule();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint("TestCqrsUpdatedEvent_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdatedEvent>>());
					});
				}));
			});

			var logger = new Mock<IBoltOnLogger<TestCqrsHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var logger2 = new Mock<IBoltOnLogger<TestCqrsUpdatedEventHandler>>();
			logger2.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger2.Object);

			var logger3 = new Mock<IBoltOnLogger<CqrsInterceptor>>();
			logger3.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger3.Object);

			var logger4 = new Mock<IBoltOnLogger<EventDispatcher>>();
			logger4.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger4.Object);

			var logger5 = new Mock<IBoltOnLogger<ProcessedEventPurger>>();
			logger5.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger5.Object);

			var dispatcher = new Mock<BoltOn.Bus.IBus>();
			dispatcher.Setup(d => d.PublishAsync(It.IsAny<ICqrsEvent>(), default(CancellationToken)))
				.Throws(new Exception("failed bus"));
			serviceCollection.AddSingleton(dispatcher.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			var ex = await Record.ExceptionAsync(async () => await mediator.ProcessAsync(new TestCqrsRequest { Input = "test" }));

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(1000);
			Assert.NotNull(ex);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));

			var repository = serviceProvider.GetService<IRepository<TestCqrsWriteEntity>>();
			var entity = repository.GetById("b33cac30-5595-4ada-97dd-f5f7c35c0f4c");
			Assert.NotNull(entity);
			Assert.True(entity.EventsToBeProcessed.Count == 1);
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.Events.Count == 1);
		}

		public void Dispose()
		{
			CqrsTestHelper.LoggerStatements.Clear();
			Bootstrapper
				.Instance
				.Dispose();
		}
	}

	public class TestCqrsRequest : IRequest
	{
		public string Input { get; set; }
	}

	public class TestCqrsHandler : IRequestAsyncHandler<TestCqrsRequest>,
		IRequestHandler<TestCqrsRequest>
	{
		private readonly IBoltOnLogger<TestCqrsHandler> _logger;
		private readonly IRepository<TestCqrsWriteEntity> _repository;

		public TestCqrsHandler(IBoltOnLogger<TestCqrsHandler> logger,
			IRepository<TestCqrsWriteEntity> repository)
		{
			_logger = logger;
			_repository = repository;
		}

		public void Handle(TestCqrsRequest request)
		{
			_logger.Debug($"{nameof(TestCqrsHandler)} invoked");
			var testCqrsWriteEntity = _repository.GetById("b33cac30-5595-4ada-97dd-f5f7c35c0f4c");
			testCqrsWriteEntity.ChangeInput(request);
			_repository.Update(testCqrsWriteEntity);
		}

		public async Task HandleAsync(TestCqrsRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug($"{nameof(TestCqrsHandler)} invoked");
			var testCqrsWriteEntity = await _repository.GetByIdAsync("b33cac30-5595-4ada-97dd-f5f7c35c0f4c");
			testCqrsWriteEntity.ChangeInput(request);
			await _repository.UpdateAsync(testCqrsWriteEntity, cancellationToken);
		}
	}

	public class TestCqrsWriteEntity : BaseCqrsEntity
	{
		public string Input { get; internal set; }

		public void ChangeInput(TestCqrsRequest request)
		{
			Input = request.Input;
			RaiseEvent(new TestCqrsUpdatedEvent
			{
				Id = Guid.Parse("42bc65b2-f8a6-4371-9906-e7641d9ae9cb"),
				Input = request.Input + " event",
			});
		}
	}

	public class TestCqrsReadEntity : BaseCqrsEntity
	{
		public virtual string Input { get; internal set; }

		public void UpdateInput(TestCqrsUpdatedEvent @event)
		{
			ProcessEvent(@event, e =>
			{
				Input = e.Input;
			});
		}
	}

	public class TestCqrsUpdatedEvent : EventToBeProcessed
	{
		public string Input { get; set; }
	}

	public class TestCqrsUpdatedEventHandler : IRequestAsyncHandler<TestCqrsUpdatedEvent>
	{
		private readonly IBoltOnLogger<TestCqrsUpdatedEventHandler> _logger;
		private readonly IRepository<TestCqrsReadEntity> _repository;

		public TestCqrsUpdatedEventHandler(IBoltOnLogger<TestCqrsUpdatedEventHandler> logger,
			IRepository<TestCqrsReadEntity> repository)
		{
			_logger = logger;
			_repository = repository;
		}

		public async Task HandleAsync(TestCqrsUpdatedEvent request, CancellationToken cancellationToken)
		{
			_logger.Debug($"{nameof(TestCqrsUpdatedEventHandler)} invoked");
			var testCqrsReadEntity = await _repository.GetByIdAsync(request.SourceId);
			testCqrsReadEntity.UpdateInput(request);
			await _repository.UpdateAsync(testCqrsReadEntity);
		}
	}

	public class TestCqrsWriteEntityMapping : IEntityTypeConfiguration<TestCqrsWriteEntity>
	{
		public void Configure(EntityTypeBuilder<TestCqrsWriteEntity> builder)
		{
			builder
				.ToTable("TestCqrsWriteEntity")
				.HasKey(k => k.Id);
			builder
				.HasMany(p => p.EventsToBeProcessed);
			builder
				.HasMany(p => p.ProcessedEvents);
		}
	}

	public class TestCqrsReadEntityMapping : IEntityTypeConfiguration<TestCqrsReadEntity>
	{
		public void Configure(EntityTypeBuilder<TestCqrsReadEntity> builder)
		{
			builder
				.ToTable("TestCqrsReadEntity")
				.HasKey(k => k.Id);
			builder
				.HasMany(p => p.EventsToBeProcessed);
			builder
				.HasMany(p => p.ProcessedEvents);
		}
	}

	public class EventToBeProcessedMapping : IEntityTypeConfiguration<EventToBeProcessed>
	{
		public void Configure(EntityTypeBuilder<EventToBeProcessed> builder)
		{
			builder
				.ToTable("EventToBeProcessed")
				.HasKey(k => k.Id);
		}
	}

	public class ProcessedEventMapping : IEntityTypeConfiguration<ProcessedEvent>
	{
		public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
		{
			builder
				.ToTable("ProcessedEvent")
				.HasKey(k => k.Id);
		}
	}

	public class TestCqrsRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{

			context.Container.AddDbContext<CqrsDbContext>(options =>
			{
				options.UseInMemoryDatabase("InMemoryDbCqrsDbContext");
				options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
			});

			context.Container.AddTransient<IRepository<TestCqrsWriteEntity>, EFCqrsRepository<TestCqrsWriteEntity, CqrsDbContext>>();
			context.Container.AddTransient<IRepository<TestCqrsReadEntity>, EFCqrsRepository<TestCqrsReadEntity, CqrsDbContext>>();
		}
	}

	public class TestCqrsPostRegistrationTask : IPostRegistrationTask
	{
		private readonly IServiceProvider _serviceProvider;

		public TestCqrsPostRegistrationTask(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public void Run(PostRegistrationTaskContext context)
		{
			var testDbContext = _serviceProvider.GetService<CqrsDbContext>();
			testDbContext.Database.EnsureDeleted();
			testDbContext.Database.EnsureCreated();

			testDbContext.Set<TestCqrsWriteEntity>().Add(new TestCqrsWriteEntity
			{
				Id = "b33cac30-5595-4ada-97dd-f5f7c35c0f4c",
				Input = "value to be replaced"
			});
			testDbContext.Set<TestCqrsReadEntity>().Add(new TestCqrsReadEntity
			{
				Id = "b33cac30-5595-4ada-97dd-f5f7c35c0f4c",
				Input = "value to be replaced"
			});
			testDbContext.SaveChanges();
		}
	}

	public static class CqrsTestHelper
	{
		public static List<string> LoggerStatements { get; set; } = new List<string>();
	}

	public class CqrsDbContext : BaseDbContext<CqrsDbContext>
	{
		public CqrsDbContext(DbContextOptions<CqrsDbContext> options) : base(options)
		{
		}

		protected override void ApplyConfigurations(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new TestCqrsWriteEntityMapping());
			modelBuilder.ApplyConfiguration(new TestCqrsReadEntityMapping());
			modelBuilder.ApplyConfiguration(new EventToBeProcessedMapping());
			modelBuilder.ApplyConfiguration(new ProcessedEventMapping());
		}
	}
}
