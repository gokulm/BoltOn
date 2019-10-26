using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bus.MassTransit;
using BoltOn.Logging;
using Moq;
using System.Linq;
using BoltOn.Bootstrapping;
using MassTransit;
using BoltOn.Mediator.Pipeline;
using BoltOn.Cqrs;
using BoltOn.Data.EF;
using BoltOn.Data;

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
		public async Task MediatorProcessAsync_WithCqrs_ReturnsResult()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnEFModule();
				b.BoltOnCqrsModule();
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

					cfg.ReceiveEndpoint("TestCqrsUpdated2Event_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdated2Event>>());
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

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts(); 
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			await mediator.ProcessAsync(new TestCqrsRequest { Input = "test input" });

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(1000);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsUpdatedEventHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsUpdatedEventHandler)} invoked for {nameof(TestCqrsUpdated2Event)}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event. Id: {CqrsConstants.EventId} SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.EventId} " +
											$"SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										 $"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.Event2Id} " +
											 $"SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsReadEntity)} updated. Input1: test input Input2Property1: prop1 Input2Propert2: 10"));
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);
		}

		[Fact]
		public void MediatorProcessAsync_WithCqrsAndNonAsync_ThrowsException()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnEFModule();
				b.BoltOnCqrsModule();
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

					cfg.ReceiveEndpoint("TestCqrsUpdated2Event_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdated2Event>>());
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
		public async Task MediatorProcessAsync_WithCqrsAndFailedBus_EventsDoNotGetProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnEFModule();
				b.BoltOnCqrsModule();
				b.BoltOnMassTransitBusModule();
			});

			var cqrsInterceptorLogger = MockForFailedBus(serviceCollection);

			var bus = new Mock<BoltOn.Bus.IBus>();
			var failedBusException = new Exception("failed bus");
			bus.Setup(d => d.PublishAsync(It.IsAny<ICqrsEvent>(), default))
				.Throws(failedBusException);
			serviceCollection.AddSingleton(bus.Object);

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
										$"Publishing event. Id: {CqrsConstants.EventId} SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.EventId} " +
											$"SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Dispatching failed. Id: {CqrsConstants.EventId}"));
			Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsUpdatedEventHandler)} invoked"));
			cqrsInterceptorLogger.Verify(v => v.Error(failedBusException), Times.Exactly(2));
			var repository = serviceProvider.GetService<IRepository<TestCqrsWriteEntity>>();
			var entity = repository.GetById(CqrsConstants.EntityId);
			Assert.NotNull(entity);
			Assert.True(entity.EventsToBeProcessed.Count == 2);
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);
		}

		[Fact]
		public async Task MediatorProcessAsync_WithCqrsAndFailedBusForOneEventOutOf2_SuccessfullyPublishedEventGetProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnEFModule();
				b.BoltOnCqrsModule();
				b.BoltOnMassTransitBusModule();
			});

			var cqrsInterceptorLogger = MockForFailedBus(serviceCollection);

			var bus = new Mock<BoltOn.Bus.IBus>();
			var failedBusException = new Exception("failed bus");
			bus.Setup(d => d.PublishAsync(It.Is<ICqrsEvent>(t => t.Id == CqrsConstants.EventId), default))
				.Throws(failedBusException);
			serviceCollection.AddSingleton(bus.Object);

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
										$"Publishing event. Id: {CqrsConstants.EventId} SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.EventId} " +
											$"SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Dispatching failed. Id: {CqrsConstants.EventId}"));
			Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Dispatching failed. Id: {CqrsConstants.Event2Id}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										 $"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.Event2Id} " +
											 $"SourceType: {typeof(TestCqrsWriteEntity).AssemblyQualifiedName}"));
			cqrsInterceptorLogger.Verify(v => v.Error(failedBusException), Times.Once);
			var repository = serviceProvider.GetService<IRepository<TestCqrsWriteEntity>>();
			var entity = repository.GetById(CqrsConstants.EntityId);
			Assert.NotNull(entity);
			Assert.True(entity.EventsToBeProcessed.Count == 2);
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);
		}

		[Fact]
		public async Task MediatorProcessAsync_ProcessAlreadyProcessedEvent_EventDoNotGetProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnEFModule();
			});

			var logger2 = new Mock<IBoltOnLogger<TestCqrsUpdatedEventHandler>>();
			logger2.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger2.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			await mediator.ProcessAsync(new TestCqrsUpdatedEvent
			{
				Id = Guid.Parse(CqrsConstants.AlreadyProcessedEventId),
				SourceId = CqrsConstants.EntityId
			});

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(1000);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsUpdatedEventHandler)} invoked"));
			Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsReadEntity)} updated. Input1: test Input2Property1: prop1 Input2Propert2: 10"));
		}

		private static Mock<IBoltOnLogger<CqrsInterceptor>> MockForFailedBus(ServiceCollection serviceCollection)
		{
			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint("TestCqrsUpdatedEvent_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdatedEvent>>());
					});

					cfg.ReceiveEndpoint("TestCqrsUpdated2Event_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdated2Event>>());
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

			var logger4 = new Mock<IBoltOnLogger<EventDispatcher>>();
			logger4.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger4.Object);

			var cqrsInterceptorLogger = new Mock<IBoltOnLogger<CqrsInterceptor>>();
			cqrsInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			cqrsInterceptorLogger.Setup(s => s.Error(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => cqrsInterceptorLogger.Object);

			return cqrsInterceptorLogger;
		}

		public void Dispose()
		{
			CqrsTestHelper.LoggerStatements.Clear();
			Bootstrapper
				.Instance
				.Dispose();
		}
	}
}
