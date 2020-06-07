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
		public async Task RequestorProcessAsync_WithCqrs_ReturnsResult()
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
			serviceCollection.AddTransient(s => logger.Object);

			var logger2 = new Mock<IBoltOnLogger<StudentUpdatedEventHandler>>();
			serviceCollection.AddTransient(s => logger2.Object);

			var logger3 = new Mock<IBoltOnLogger<CqrsInterceptor>>();
			serviceCollection.AddTransient(s => logger3.Object);

			var logger4 = new Mock<IBoltOnLogger<EventDispatcher>>();
			serviceCollection.AddTransient((s) => logger4.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();
			var cqrsDbContext = serviceProvider.GetService<CqrsDbContext>();

			// act
			await requestor.ProcessAsync(new UpdateStudentRequest { Input = "test input" });

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(100);
			logger.Verify(v => v.Debug($"{nameof(UpdateStudentHandler)} invoked"));
			logger2.Verify(v => v.Debug($"{nameof(StudentUpdatedEventHandler)} invoked"));
			logger2.Verify(v => v.Debug($"{nameof(StudentUpdatedEventHandler)} invoked for {nameof(TestCqrsUpdated2Event)}"));
			logger3.Verify(v => v.Debug($"Publishing event. Id: {CqrsConstants.EventId} SourceType: {typeof(Student).AssemblyQualifiedName}"));
			logger4.Verify(v => v.Debug($"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.EventId} " +
											$"SourceType: {typeof(Student).AssemblyQualifiedName}"));
			logger4.Verify(v => v.Debug($"Publishing event to bus from EventDispatcher. Id: {CqrsConstants.Event2Id} " +
											 $"SourceType: {typeof(Student).AssemblyQualifiedName}"));
			logger2.Verify(v => v.Debug($"{nameof(StudentFlattened)} updated. Input1: test input Input2Property1: prop1 Input2Propert2: 10"));
			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);
			var studentFlattened = cqrsDbContext.Set<StudentFlattened>().Find(CqrsConstants.EntityId);
			Assert.True(studentFlattened.EventsToBeProcessed.Count() == 0);
			Assert.True(studentFlattened.ProcessedEvents.Count() > 0);
		}

		[Fact]
		public async Task RequestorProcessAsync_WithCqrsAndPurgeEventsToBeProcessedDisabled_DoesNotRemoveEventsToBeProcessed()
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
				}));
			});

			var logger = new Mock<IBoltOnLogger<StudentCreatedEventHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();
			var studentId = Guid.NewGuid();

			// act
			await requestor.ProcessAsync(new AddStudentRequest { Id = studentId, Name = "test input" });

			// assert
			await Task.Delay(100);
			logger.Verify(v => v.Debug($"{nameof(StudentCreatedEventHandler)} invoked"));

			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);

			var cqrsDbContext = serviceProvider.GetService<CqrsDbContext>();
			var student = cqrsDbContext.Set<Student>().Find(studentId);
			Assert.True(student.EventsToBeProcessed.Any());
			Assert.True(!student.ProcessedEvents.Any());
			var studentFlattened = cqrsDbContext.Set<StudentFlattened>().Find(studentId);
			Assert.True(studentFlattened.EventsToBeProcessed.Count() == 0);
			Assert.True(studentFlattened.ProcessedEvents.Count() > 0);
		}

		[Fact]
		public async Task RequestorProcessAsync_WithCqrsAndPurgeEventsToBeProcessedEnabled_RemovesEventsToBeProcessed()
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
			serviceCollection.AddTransient((s) => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();
			var studentId = Guid.NewGuid();

			// act
			await requestor.ProcessAsync(new AddStudentRequest { Id = studentId, Name = "test input", RaiseAnotherCreateEvent = false });

			// assert
			await Task.Delay(1000);
			logger.Verify(v => v.Debug($"{nameof(StudentCreatedEventHandler)} invoked"));

			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);

			var cqrsDbContext = serviceProvider.GetService<CqrsDbContext>();
			var student = cqrsDbContext.Set<Student>().Find(studentId);
			Assert.True(student.EventsToBeProcessed.Count() == 0);
			Assert.True(student.ProcessedEvents.Count() == 0);
			var studentFlattened = cqrsDbContext.Set<StudentFlattened>().Find(studentId);
			Assert.True(studentFlattened.EventsToBeProcessed.Count() == 0);
			Assert.True(studentFlattened.ProcessedEvents.Count() > 0);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		public async Task RequestorProcessAsync_WithPurgeProcessedEventsWithDifferentProcessedBefore_RemovesAndNotRemovesProcessedEvents(int purgeEventsProcessedBefore)
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnCqrsModule(c => c.PurgeEventsProcessedBefore = TimeSpan.FromSeconds(purgeEventsProcessedBefore));
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
			serviceCollection.AddTransient((s) => logger.Object);

			var logger2 = new Mock<IBoltOnLogger<CqrsInterceptor>>();
			serviceCollection.AddSingleton((s) => logger2.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();
			var studentId = Guid.NewGuid();

			// act
			await requestor.ProcessAsync(new AddStudentRequest { Id = studentId, Name = "test input", RaiseAnotherCreateEvent = false });

			// assert
			await Task.Delay(100);
			logger.Verify(v => v.Debug($"{nameof(StudentCreatedEventHandler)} invoked"));
			logger2.Verify(v => v.Debug("Removed event"));

			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count == 0);

			var cqrsDbContext = serviceProvider.GetService<CqrsDbContext>();
			var student = cqrsDbContext.Set<Student>().Find(studentId);
			Assert.True(student.EventsToBeProcessed.Count() == 0);
			Assert.True(student.ProcessedEvents.Count() == 0);
			var studentFlattened = cqrsDbContext.Set<StudentFlattened>().Find(studentId);
			Assert.True(studentFlattened.EventsToBeProcessed.Count() == 0);
			if (purgeEventsProcessedBefore == 1)
				Assert.True(studentFlattened.ProcessedEvents.Count() == 0);
			else
				Assert.True(studentFlattened.ProcessedEvents.Count() > 0);
		}

		[Fact]
		public async Task RequestorProcessAsync_WithPurgeEventsToBeProcessedEnabledAndFailedPurging_DoesNotRemoveEventsToBeProcessed()
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
			serviceCollection.AddTransient((s) => logger.Object);

			var logger2 = new Mock<IBoltOnLogger<CqrsInterceptor>>();
			logger2.Setup(s => s.Debug(It.Is<string>(s => s.StartsWith("Removing event. Id:")))).Throws(new Exception());
			serviceCollection.AddSingleton((s) => logger2.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();
			var studentId = Guid.NewGuid();

			// act
			await requestor.ProcessAsync(new AddStudentRequest { Id = studentId, Name = "test input" });

			// assert
			await Task.Delay(300);
			logger.Verify(v => v.Debug($"{nameof(StudentCreatedEventHandler)} invoked"));

			var eventBag = serviceProvider.GetService<EventBag>();
			Assert.True(eventBag.EventsToBeProcessed.Count > 0);

			logger2.Verify(v => v.Error(It.Is<string>(f => f.StartsWith("Dispatching or purging failed. Event Id:"))));
			var cqrsDbContext = serviceProvider.GetService<CqrsDbContext>();
			var student = cqrsDbContext.Set<Student>().Find(studentId);
			Assert.True(student.EventsToBeProcessed.Count() > 0);
			Assert.True(student.ProcessedEvents.Count() == 0);
			var studentFlattened = cqrsDbContext.Set<StudentFlattened>().Find(studentId);
			Assert.True(studentFlattened.EventsToBeProcessed.Count() == 0);
			Assert.True(studentFlattened.ProcessedEvents.Count() > 0);
		}

		[Fact]
		public async Task RequestorProcessAsync_WithCqrsAndFailedBus_EventsDoNotGetProcessed()
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
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			await requestor.ProcessAsync(new UpdateStudentRequest { Input = "test" });

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
										$"Dispatching or purging failed. Event Id: {CqrsConstants.EventId}"));
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
		public async Task RequestorProcessAsync_WithCqrsAndFailedBusFor1stEventOutOf2_BothEventsDoNotGetProcessed()
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
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			await requestor.ProcessAsync(new UpdateStudentRequest { Input = "test" });

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
										$"Dispatching or purging failed. Event Id: {CqrsConstants.EventId}"));
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
		public async Task RequestorProcessAsync_WithCqrsAndFailedBusFor2ndEventOutOf2Events_FirstEventGetProcessed()
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
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			await requestor.ProcessAsync(new UpdateStudentRequest { Input = "test" });

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
										$"Dispatching or purging failed. Event Id: {CqrsConstants.EventId}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"Dispatching or purging failed. Event Id: {CqrsConstants.Event2Id}"));
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
		public async Task RequestorProcessAsync_ProcessAlreadyProcessedEvent_EventDoNotGetProcessed()
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
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			await requestor.ProcessAsync(new StudentUpdatedEvent
			{
				Id = Guid.Parse(CqrsConstants.AlreadyProcessedEventId),
				SourceId = CqrsConstants.EntityId
			});

			// assert
			// as assert not working after async method, added sleep
			await Task.Delay(100);
			logger2.Verify(v => v.Debug($"{nameof(StudentUpdatedEventHandler)} invoked"));
			logger2.Verify(v => v.Debug($"{nameof(StudentFlattened)} updated. Input1: test Input2Property1: " +
				"prop1 Input2Propert2: 10"), Times.Never);
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
