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
										$"Publishing event. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb SourceType: {typeof(TestCqrsEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb SourceType: {typeof(TestCqrsEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Building repository. SourceType: {typeof(TestCqrsEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Removed event. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										"Fetched BaseCqrsEntity. Id: b33cac30-5595-4ada-97dd-f5f7c35c0f4c"));
			var repository = serviceProvider.GetService<IRepository<TestCqrsEntity>>();
			var entity = repository.GetById("b33cac30-5595-4ada-97dd-f5f7c35c0f4c");
			Assert.True(entity.Events.Count == 0);
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
										$"Publishing event. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb SourceType: {typeof(TestCqrsEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: 42bc65b2-f8a6-4371-9906-e7641d9ae9cb SourceType: {typeof(TestCqrsEntity).AssemblyQualifiedName}"));

			var repository = serviceProvider.GetService<IRepository<TestCqrsEntity>>();
			var entity = repository.GetById("b33cac30-5595-4ada-97dd-f5f7c35c0f4c");
			Assert.NotNull(entity);
			Assert.True(entity.Events.Count == 1);
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
		private readonly IRepository<TestCqrsEntity> _repository;

		public TestCqrsHandler(IBoltOnLogger<TestCqrsHandler> logger,
			IRepository<TestCqrsEntity> repository)
		{
			_logger = logger;
			_repository = repository;
		}

		public void Handle(TestCqrsRequest request)
		{
			_logger.Debug($"{nameof(TestCqrsHandler)} invoked");
			var testCqrsEntity = new TestCqrsEntity { Id = "b33cac30-5595-4ada-97dd-f5f7c35c0f4c" };
			testCqrsEntity.Update(request);
			_repository.Add(testCqrsEntity);
		}

		public async Task HandleAsync(TestCqrsRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug($"{nameof(TestCqrsHandler)} invoked");
			var testCqrsEntity = new TestCqrsEntity { Id = "b33cac30-5595-4ada-97dd-f5f7c35c0f4c" };
			testCqrsEntity.Update(request);
			await _repository.AddAsync(testCqrsEntity);
		}
	}

	public class TestCqrsEntity : BaseCqrsEntity
	{
		public string Input { get; set; }

		public void Update(TestCqrsRequest request)
		{
			Input = request.Input;
			RaiseEvent(new TestCqrsUpdatedEvent { Input = request.Input + " event", SourceId = Id, Id = Guid.Parse("42bc65b2-f8a6-4371-9906-e7641d9ae9cb") });
		}
	}

	public class TestCqrsUpdatedEvent : CqrsEvent
	{
		public string Input { get; set; }
	}


	public class TestCqrsUpdatedEventHandler : IRequestAsyncHandler<TestCqrsUpdatedEvent>
	{
		private readonly IBoltOnLogger<TestCqrsUpdatedEventHandler> _logger;

		public TestCqrsUpdatedEventHandler(IBoltOnLogger<TestCqrsUpdatedEventHandler> logger)
		{
			_logger = logger;
		}

		public async Task HandleAsync(TestCqrsUpdatedEvent request, CancellationToken cancellationToken)
		{
			_logger.Debug($"{nameof(TestCqrsUpdatedEventHandler)} invoked");
			await Task.FromResult(1);
		}
	}

	public class TestCqrsEntityMapping : IEntityTypeConfiguration<TestCqrsEntity>
	{
		public void Configure(EntityTypeBuilder<TestCqrsEntity> builder)
		{
			builder
				.ToTable("TestCqrsEntity")
				.HasKey(k => k.Id);
			builder
				.HasMany(p => p.Events);
		}
	}

	public class BoltOnEventMapping : IEntityTypeConfiguration<CqrsEvent>
	{
		public void Configure(EntityTypeBuilder<CqrsEvent> builder)
		{
			builder
				.ToTable("BoltOnEvent")
				.HasKey(k => k.Id);
		}
	}

	public class TestCqrsRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			context.Container.AddTransient<IRepository<TestCqrsEntity>, EFCqrsRepository<TestCqrsEntity, SchoolDbContext>>();
		}
	}

	public static class CqrsTestHelper
	{
		public static List<string> LoggerStatements { get; set; } = new List<string>();
	}
}