using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bus.MassTransit;
using BoltOn.Logging;
using Moq;
using System.Linq;
using MassTransit;
using BoltOn.Cqrs;
using BoltOn.Data.EF;
using BoltOn.Data;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Cqrs.Fakes;
using AddStudentRequest = BoltOn.Tests.Cqrs.Fakes.AddStudentRequest;

namespace BoltOn.Tests.Cqrs
{
	[Collection("IntegrationTests")]
	public class CqrsIntegrationTests : IDisposable
	{
		[Fact]
		public async Task MediatorProcessAsync_WithCqrs_ReturnsResult()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnCqrsModule();
				b.BoltOnMassTransitBusModule();
				b.RegisterCqrsFakes();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint($"{nameof(StudentUpdatedEvent)}_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentUpdatedEvent>>());
					});

					cfg.ReceiveEndpoint($"{nameof(TestCqrsUpdated2Event)}_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdated2Event>>());
					});
				}));
			});

			var logger = new Mock<IBoltOnLogger<UpdateStudentHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient(s => logger.Object);

			var logger2 = new Mock<IBoltOnLogger<StudentUpdatedEventHandler>>();
			logger2.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient(s => logger2.Object);

			var logger3 = new Mock<IBoltOnLogger<CqrsInterceptor>>();
			logger3.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient(s => logger3.Object);

			var logger4 = new Mock<IBoltOnLogger<EventDispatcher>>();
			logger4.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger4.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts(); 
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			await mediator.ProcessAsync(new UpdateStudentRequest { Input = "test input" });

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(100);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(UpdateStudentHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(StudentUpdatedEventHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(StudentUpdatedEventHandler)} invoked for {nameof(TestCqrsUpdated2Event)}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event. Id: {CqrsConstants.EventId} SourceType: {typeof(Student).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.EventId} " +
											$"SourceType: {typeof(Student).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										 $"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.Event2Id} " +
											 $"SourceType: {typeof(Student).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(StudentFlattened)} updated. Input1: test input Input2Property1: prop1 Input2Propert2: 10"));
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);
			Assert.True(eventBag.ProcessedEvents.Count > 0);
		}

		[Fact]
		public async Task MediatorProcessAsync_WithCqrsAndPurgeEventsToBeProcessedDisabled_DoesNotRemoveEventsToBeProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnCqrsModule(p => p.PurgeEventsToBeProcessed = false);
				b.BoltOnMassTransitBusModule();
                b.RegisterCqrsFakes();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint($"{nameof(StudentCreatedEvent)}_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentCreatedEvent>>());
					});

					cfg.ReceiveEndpoint($"{nameof(CqrsEventProcessedEvent)}_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<CqrsEventProcessedEvent>>());
					});
				}));
			});

			var logger = new Mock<IBoltOnLogger<StudentCreatedEventHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var mediator = serviceProvider.GetService<IMediator>();
			var studentId = Guid.NewGuid();

			// act
			await mediator.ProcessAsync(new AddStudentRequest { Id = studentId, Name = "test input" });

			// assert
			await Task.Delay(100);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(StudentCreatedEventHandler)} invoked"));

			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);
			Assert.True(eventBag.ProcessedEvents.Count > 0);

			var cqrsDbContext = serviceProvider.GetService<CqrsDbContext>();
			var student = cqrsDbContext.Set<Student>().Find(studentId);
			Assert.True(student.EventsToBeProcessed.Any());
			Assert.True(!student.ProcessedEvents.Any());
			var studentFlattened = cqrsDbContext.Set<StudentFlattened>().Find(studentId);
			Assert.True(studentFlattened.EventsToBeProcessed.Count() == 0);
			Assert.True(studentFlattened.ProcessedEvents.Count() > 0);
		}

		[Fact]
		public async Task MediatorProcessAsync_WithCqrsAndPurgeEventsToBeProcessedEnabled_RemovesEventsToBeProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnCqrsModule();
				b.BoltOnMassTransitBusModule();
                b.RegisterCqrsFakes();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint($"{nameof(StudentCreatedEvent)}_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentCreatedEvent>>());
					});
				}));
			});

			var logger = new Mock<IBoltOnLogger<StudentCreatedEventHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var logger2 = new Mock<IBoltOnLogger<IEventPurger>>();
			logger2.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddSingleton((s) => logger2.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var mediator = serviceProvider.GetService<IMediator>();
			var studentId = Guid.NewGuid();

			// act
			await mediator.ProcessAsync(new AddStudentRequest { Id = studentId, Name = "test input", RaiseAnotherCreateEvent = false });

			// assert
			await Task.Delay(100);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(StudentCreatedEventHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Getting entity repository. TypeName: {typeof(Student).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Fetching entity by Id. Id: {studentId}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Fetched entity. Id: {studentId}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == "Removed event"));

			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);
			Assert.True(eventBag.ProcessedEvents.Count > 0);

			var cqrsDbContext = serviceProvider.GetService<CqrsDbContext>();
			var student = cqrsDbContext.Set<Student>().Find(studentId);
			Assert.True(student.EventsToBeProcessed.Count() == 0);
			Assert.True(student.ProcessedEvents.Count() == 0);
			var studentFlattened = cqrsDbContext.Set<StudentFlattened>().Find(studentId);
			Assert.True(studentFlattened.EventsToBeProcessed.Count() == 0);
			Assert.True(studentFlattened.ProcessedEvents.Count() > 0);
		}

		[Fact]
		public async Task MediatorProcessAsync_WithPurgeEventsToBeProcessedEnabledAndFailedEventPurger_DoesNotRemoveEventsToBeProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnCqrsModule();
				b.BoltOnMassTransitBusModule();
                b.RegisterCqrsFakes();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint($"{nameof(StudentCreatedEvent)}_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentCreatedEvent>>());
					});

					cfg.ReceiveEndpoint($"{nameof(CqrsEventProcessedEvent)}_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<CqrsEventProcessedEvent>>());
					});
				}));
			});

			var logger = new Mock<IBoltOnLogger<StudentCreatedEventHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var cqrsRepositoryFactory = new Mock<ICqrsRepositoryFactory>();
			cqrsRepositoryFactory.Setup(s => s.GetRepository<Student>()).Throws(new Exception());
			serviceCollection.AddSingleton(cqrsRepositoryFactory.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var mediator = serviceProvider.GetService<IMediator>();
			var studentId = Guid.NewGuid();

			// act
			await mediator.ProcessAsync(new AddStudentRequest { Id = studentId, Name = "test input" });

			// assert
			await Task.Delay(300);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(StudentCreatedEventHandler)} invoked"));

			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);
			Assert.True(eventBag.ProcessedEvents.Count > 0);

			var cqrsDbContext = serviceProvider.GetService<CqrsDbContext>();
			var student = cqrsDbContext.Set<Student>().Find(studentId);
			Assert.True(student.EventsToBeProcessed.Count() > 0);
			Assert.True(student.ProcessedEvents.Count() == 0);
			var studentFlattened = cqrsDbContext.Set<StudentFlattened>().Find(studentId);
			Assert.True(studentFlattened.EventsToBeProcessed.Count() == 0);
			Assert.True(studentFlattened.ProcessedEvents.Count() > 0);
		}
		
		[Fact]
		public async Task MediatorProcessAsync_WithCqrsAndFailedBus_EventsDoNotGetProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnCqrsModule();
				b.BoltOnMassTransitBusModule();
                b.RegisterCqrsFakes();
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
			await mediator.ProcessAsync(new UpdateStudentRequest { Input = "test" });

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(100);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(UpdateStudentHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event. Id: {CqrsConstants.EventId} SourceType: {typeof(Student).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.EventId} " +
											$"SourceType: {typeof(Student).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Dispatching failed. Id: {CqrsConstants.EventId}"));
			Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(StudentUpdatedEventHandler)} invoked"));
			cqrsInterceptorLogger.Verify(v => v.Error(failedBusException), Times.Once);
			var repository = serviceProvider.GetService<IRepository<Student>>();
			var entity = await repository.GetByIdAsync(CqrsConstants.EntityId);
			Assert.NotNull(entity);
			Assert.True(entity.EventsToBeProcessed.ToList().Count == 2);
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 2);
		}

		[Fact]
		public async Task MediatorProcessAsync_WithCqrsAndFailedBusFor1stEventOutOf2_BothEventsDoNotGetProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnCqrsModule();
				b.BoltOnMassTransitBusModule();
                b.RegisterCqrsFakes();
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
			await mediator.ProcessAsync(new UpdateStudentRequest { Input = "test" });

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(100);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(UpdateStudentHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event. Id: {CqrsConstants.EventId} SourceType: {typeof(Student).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.EventId} " +
											$"SourceType: {typeof(Student).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Dispatching failed. Id: {CqrsConstants.EventId}"));
			Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										 $"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.Event2Id} " +
											 $"SourceType: {typeof(Student).AssemblyQualifiedName}"));
			cqrsInterceptorLogger.Verify(v => v.Error(failedBusException), Times.Once);
			var repository = serviceProvider.GetService<IRepository<Student>>();
			var entity = await repository.GetByIdAsync(CqrsConstants.EntityId);
			Assert.NotNull(entity);
			Assert.True(entity.EventsToBeProcessed.ToList().Count == 2);
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 2);
		}

		[Fact]
		public async Task MediatorProcessAsync_WithCqrsAndFailedBusFor2ndEventOutOf2Events_FirstEventGetProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnCqrsModule(b => b.PurgeEventsToBeProcessed = false);
				b.BoltOnMassTransitBusModule();
                b.RegisterCqrsFakes();
			});

			var cqrsInterceptorLogger = MockForFailedBus(serviceCollection);

			var bus = new Mock<BoltOn.Bus.IBus>();
			var failedBusException = new Exception("failed bus");
			bus.Setup(d => d.PublishAsync(It.Is<ICqrsEvent>(t => t.Id == CqrsConstants.Event2Id), default))
				.Throws(failedBusException);
			serviceCollection.AddSingleton(bus.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			await mediator.ProcessAsync(new UpdateStudentRequest { Input = "test" });

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(100);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(UpdateStudentHandler)} invoked"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event. Id: {CqrsConstants.EventId} SourceType: {typeof(Student).AssemblyQualifiedName}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.EventId} " +
											$"SourceType: {typeof(Student).AssemblyQualifiedName}"));
			Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Dispatching failed. Id: {CqrsConstants.EventId}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Dispatching failed. Id: {CqrsConstants.Event2Id}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										 $"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.Event2Id} " +
											 $"SourceType: {typeof(Student).AssemblyQualifiedName}"));
			cqrsInterceptorLogger.Verify(v => v.Error(failedBusException), Times.Once);
			var repository = serviceProvider.GetService<IRepository<Student>>();
			var entity = await repository.GetByIdAsync(CqrsConstants.EntityId);
			Assert.NotNull(entity);
			Assert.True(entity.EventsToBeProcessed.ToList().Count == 2);
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 1);
		}

		[Fact]
		public async Task MediatorProcessAsync_ProcessAlreadyProcessedEvent_EventDoNotGetProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
                b.BoltOnCqrsModule();
                b.RegisterCqrsFakes();
			});

			var logger2 = new Mock<IBoltOnLogger<StudentUpdatedEventHandler>>();
			logger2.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient(s => logger2.Object);

			var bus = new Mock<BoltOn.Bus.IBus>();
			serviceCollection.AddSingleton(bus.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			await mediator.ProcessAsync(new StudentUpdatedEvent
			{
				Id = Guid.Parse(CqrsConstants.AlreadyProcessedEventId),
				SourceId = CqrsConstants.EntityId
			});

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(100);
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(StudentUpdatedEventHandler)} invoked"));
			Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(StudentFlattened)} updated. Input1: test Input2Property1: prop1 Input2Propert2: 10"));
		}

		private static Mock<IBoltOnLogger<CqrsInterceptor>> MockForFailedBus(ServiceCollection serviceCollection)
		{
			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint("TestCqrsUpdatedEvent_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentUpdatedEvent>>());
					});

					cfg.ReceiveEndpoint("TestCqrsUpdated2Event_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdated2Event>>());
					});
				}));
			});

			var logger = new Mock<IBoltOnLogger<UpdateStudentHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var logger2 = new Mock<IBoltOnLogger<StudentUpdatedEventHandler>>();
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
		}
	}
}
