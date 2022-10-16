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
using System.Linq.Expressions;
using System.Threading;

namespace BoltOn.Tests.Cqrs
{
	[Collection("IntegrationTests")]
	public class CqrsIntegrationTests : IDisposable
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task RequestorProcessAsync_AddEntityAndPurgeOrNotPurgeEvents_AddsEntityAndRemoveOrNotRemoveEventsFromEventStore(bool purgeEvents)
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnMassTransitBusModule();
				b.RegisterCqrsFakes();
			});

			serviceCollection.AddMassTransit(x =>
			{
				//x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg => { }));

				x.AddConsumer<AppMessageConsumer<StudentCreatedEvent>>();
				x.SetKebabCaseEndpointNameFormatter();

				x.UsingInMemory((context, cfg) =>
				{
					cfg.ConfigureEndpoints(context);
				});
			});

			var loggerFactory = new Mock<IAppLoggerFactory>();
			var cqrsRepositoryLogger = new Mock<IAppLogger<CqrsRepository<Student, SchoolDbContext>>>();
			loggerFactory.Setup(s => s.Create<CqrsRepository<Student, SchoolDbContext>>())
				.Returns(cqrsRepositoryLogger.Object);
			serviceCollection.AddTransient((s) => loggerFactory.Object);

			var logger = new Mock<IAppLogger<StudentCreatedEventHandler>>();
			serviceCollection.AddTransient((s) => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();
			var studentId = Guid.NewGuid();

			// act
			await requestor.ProcessAsync(new AddStudentRequest { StudentId = studentId, Name = "test input", PurgeEvents = purgeEvents });

			// assert
			await Task.Delay(500);
			cqrsRepositoryLogger.Verify(v => v.Debug("Adding entities. Count: 1"));
			cqrsRepositoryLogger.Verify(v => v.Debug("Transaction complete. Added entities."));
			if(purgeEvents)
			{
				cqrsRepositoryLogger.Verify(v => v.Debug("Publishing events and purging. No. of events to be purged: 1"));
				cqrsRepositoryLogger.Verify(v => v.Debug($"Removed event. EventId: {CqrsConstants.Event1Id}"));
				cqrsRepositoryLogger.Verify(v => v.Debug($"Published event. EventId: {CqrsConstants.Event1Id}"));
				cqrsRepositoryLogger.Verify(v => v.Debug($"Transaction complete. Removed and published successfully. EventId: {CqrsConstants.Event1Id}"));
			}
			else
			{
				cqrsRepositoryLogger.Verify(v => v.Debug("Publishing events and updating. No. of events to be updated: 1"));
				cqrsRepositoryLogger.Verify(v => v.Debug($"Updated event. EventId: {CqrsConstants.Event1Id}"));
				cqrsRepositoryLogger.Verify(v => v.Debug($"Published event. EventId: {CqrsConstants.Event1Id}"));
				cqrsRepositoryLogger.Verify(v => v.Debug($"Transaction complete. Updated and published successfully. EventId: {CqrsConstants.Event1Id}"));
			}

			logger.Verify(v => v.Debug($"{nameof(StudentCreatedEventHandler)} invoked"));

			var schoolDbContext = serviceProvider.GetService<SchoolDbContext>();
			var student = schoolDbContext.Set<Student>().Find(studentId);
			Assert.NotNull(student);
			if (purgeEvents)
				Assert.False(student.EventsToBeProcessed.Any());
			else
				Assert.True(student.EventsToBeProcessed.Any());

			var studentFlattened = schoolDbContext.Set<StudentFlattened>().Find(studentId);
			//Assert.NotNull(studentFlattened);
			var eventStore = schoolDbContext.Set<EventStore>().FirstOrDefault(w => w.EntityId == studentId.ToString());
			if (purgeEvents)
				Assert.Null(eventStore);
			else
				Assert.NotNull(eventStore);
		}

		[Fact]
		public async Task RequestorProcessAsync_WithPurgeEventsToBeProcessedEnabledAndFailedPurging_DoesNotRemoveEventsToBeProcessedAndDoesNotPublish()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnMassTransitBusModule();
				b.RegisterCqrsFakes();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddConsumer<AppMessageConsumer<StudentCreatedEvent>>()
					.Endpoint(e =>
					{
						e.Name = $"{nameof(StudentCreatedEvent)}_queue";
					});

				x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
			});

			var logger = new Mock<IAppLogger<StudentCreatedEventHandler>>();
			serviceCollection.AddTransient((s) => logger.Object);

			var loggerFactory = new Mock<IAppLoggerFactory>();
			var cqrsRepositoryLogger = new Mock<IAppLogger<CqrsRepository<Student, SchoolDbContext>>>();
			loggerFactory.Setup(s => s.Create<CqrsRepository<Student, SchoolDbContext>>())
				.Returns(cqrsRepositoryLogger.Object);
			serviceCollection.AddTransient((s) => loggerFactory.Object);

			var studentId = Guid.NewGuid();
			var eventStoreRepository = new Mock<IRepository<EventStore>>();
			var failedEventRepositoryException = new Exception("failed event repository");
			eventStoreRepository.Setup(d => d.FindByAsync(It.IsAny<Expression<Func<EventStore, bool>>>(), It.IsAny<CancellationToken>()))
				.Throws(failedEventRepositoryException);
			serviceCollection.AddSingleton(eventStoreRepository.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			var exception = await Record.ExceptionAsync(async () => await requestor.ProcessAsync(new AddStudentRequest { StudentId = studentId, Name = "test input" }));

			// assert
			cqrsRepositoryLogger.Verify(v => v.Debug("Publishing events and purging. No. of events to be purged: 1"), Times.Never);
			Assert.NotNull(exception);
			var schoolDbContext = serviceProvider.GetService<SchoolDbContext>();
			var student = schoolDbContext.Set<Student>().Find(studentId);
			Assert.True(student.EventsToBeProcessed.Count() > 0);
			var studentFlattened = schoolDbContext.Set<StudentFlattened>().Find(studentId);
			Assert.Null(studentFlattened);
			logger.Verify(v => v.Debug($"{nameof(StudentCreatedEventHandler)} invoked"), Times.Never);
		}

		[Fact]
		public async Task RequestorProcessAsync_WithPurgeEventsToBeProcessedEnabledAndFailedBus_DoesNotRemoveEventsToBeProcessedAndDoesNotPublish()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.BoltOnMassTransitBusModule();
				b.RegisterCqrsFakes();
			});

			serviceCollection.AddMassTransit(x =>
			{
				x.AddConsumer<AppMessageConsumer<StudentCreatedEvent>>()
					.Endpoint(e =>
					{
						e.Name = $"{nameof(StudentCreatedEvent)}_queue";
					});

				x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
			});

			var logger = new Mock<IAppLogger<StudentCreatedEventHandler>>();
			serviceCollection.AddTransient((s) => logger.Object);

			var loggerFactory = new Mock<IAppLoggerFactory>();
			var cqrsRepositoryLogger = new Mock<IAppLogger<CqrsRepository<Student, SchoolDbContext>>>();
			loggerFactory.Setup(s => s.Create<CqrsRepository<Student, SchoolDbContext>>())
				.Returns(cqrsRepositoryLogger.Object);
			serviceCollection.AddTransient((s) => loggerFactory.Object);

			var studentId = Guid.NewGuid();
			var bus = new Mock<BoltOn.Bus.IAppServiceBus>();
			var failedBusException = new Exception("failed bus");
			bus.Setup(d => d.PublishAsync(It.IsAny<IDomainEvent>(), default))
				.Throws(failedBusException);
			serviceCollection.AddSingleton(bus.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			var exception = await Record.ExceptionAsync(async () => await requestor.ProcessAsync(new AddStudentRequest { StudentId = studentId, Name = "test input" }));

			// assert
			cqrsRepositoryLogger.Verify(v => v.Debug("Publishing events and purging. No. of events to be purged: 1"));
			cqrsRepositoryLogger.Verify(v => v.Debug($"Removed event. EventId: {CqrsConstants.Event1Id}"));
			cqrsRepositoryLogger.Verify(v => v.Debug($"Published event. EventId: {CqrsConstants.Event1Id}"), Times.Never);

			Assert.NotNull(exception);
			var schoolDbContext = serviceProvider.GetService<SchoolDbContext>();
			var student = schoolDbContext.Set<Student>().Find(studentId);
			Assert.NotNull(student);
			var test = schoolDbContext.Set<EventStore>().ToList();
			//var eventStore = schoolDbContext.Set<EventStore>().FirstOrDefault(w => w.EntityId == studentId.ToString() &&
			//					w.EntityType == typeof(Student).FullName);
			//Assert.NotNull(eventStore);
			var studentFlattened = schoolDbContext.Set<StudentFlattened>().Find(studentId);
			Assert.Null(studentFlattened);
			logger.Verify(v => v.Debug($"{nameof(StudentCreatedEventHandler)} invoked"), Times.Never);
		}

		[Fact]
		public async Task RequestorProcessAsync_WithCqrsAndFailedBusFor1stEventOutOf2_BothEventsDoNotGetProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.RegisterCqrsFakes();
			});

			var loggerFactory = new Mock<IAppLoggerFactory>();
			var cqrsRepositoryLogger = new Mock<IAppLogger<CqrsRepository<Student, SchoolDbContext>>>();
			loggerFactory.Setup(s => s.Create<CqrsRepository<Student, SchoolDbContext>>())
				.Returns(cqrsRepositoryLogger.Object);
			serviceCollection.AddTransient((s) => loggerFactory.Object);

			var appServiceLogger = new Mock<IAppLogger<AppServiceBus>>();
			serviceCollection.AddTransient((s) => appServiceLogger.Object);

			var studentId = Guid.NewGuid();
			var bus = new Mock<BoltOn.Bus.IAppServiceBus>();
			var failedBusException = new Exception("failed bus");
			bus.Setup(d => d.PublishAsync(It.Is<IDomainEvent>(i => i.EventId == CqrsConstants.Event1Id), default))
				.Throws(failedBusException);
			serviceCollection.AddSingleton(bus.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			var exception = await Record.ExceptionAsync(async () => await requestor.ProcessAsync(
				new AddStudentRequest { StudentId = studentId, Name = "test input", RaiseAnotherCreateEvent = true,
					EventId = CqrsConstants.Event1Id
				}));

			// assert
			Assert.NotNull(exception);
			var schoolDbContext = serviceProvider.GetService<SchoolDbContext>();
			var student = schoolDbContext.Set<Student>().Find(studentId);
			Assert.NotNull(student);
			bus.Verify(v => v.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
			cqrsRepositoryLogger.Verify(v => v.Debug("Publishing events and purging. No. of events to be purged: 1"));
			cqrsRepositoryLogger.Verify(v => v.Debug($"Removed event. EventId: {CqrsConstants.Event1Id}")); 
			cqrsRepositoryLogger.Verify(v => v.Debug($"Published event. EventId: {CqrsConstants.Event1Id}"), Times.Never);
			cqrsRepositoryLogger.Verify(v => v.Debug($"Published event. EventId: {CqrsConstants.Event2Id}"), Times.Never);
		}

		[Fact]
		public async Task RequestorProcessAsync_WithCqrsAndFailedBusFor2ndtEventOutOf2_FirstEventGetsProcessed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnEFModule();
				b.RegisterCqrsFakes();
			});

			var loggerFactory = new Mock<IAppLoggerFactory>();
			var cqrsRepositoryLogger = new Mock<IAppLogger<CqrsRepository<Student, SchoolDbContext>>>();
			loggerFactory.Setup(s => s.Create<CqrsRepository<Student, SchoolDbContext>>())
				.Returns(cqrsRepositoryLogger.Object);
			serviceCollection.AddTransient((s) => loggerFactory.Object);

			var appServiceLogger = new Mock<IAppLogger<AppServiceBus>>();
			serviceCollection.AddTransient((s) => appServiceLogger.Object);

			var studentId = Guid.NewGuid();
			var bus = new Mock<BoltOn.Bus.IAppServiceBus>();
			var failedBusException = new Exception("failed bus");
			bus.Setup(d => d.PublishAsync(It.Is<IDomainEvent>(i => i.EventId == CqrsConstants.Event2Id), default))
				.Throws(failedBusException);
			serviceCollection.AddSingleton(bus.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var requestor = serviceProvider.GetService<IRequestor>();

			// act
			var exception = await Record.ExceptionAsync(async () => await requestor.ProcessAsync(
				new AddStudentRequest { StudentId = studentId, Name = "test input", RaiseAnotherCreateEvent = true,
										EventId = CqrsConstants.Event2Id}));

			// assert
			Assert.NotNull(exception);
			var schoolDbContext = serviceProvider.GetService<SchoolDbContext>();
			var student = schoolDbContext.Set<Student>().Find(studentId);
			Assert.NotNull(student);
			bus.Verify(v => v.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
			cqrsRepositoryLogger.Verify(v => v.Debug("Publishing events and purging. No. of events to be purged: 1"));
			cqrsRepositoryLogger.Verify(v => v.Debug($"Removed event. EventId: {CqrsConstants.Event1Id}"));
			cqrsRepositoryLogger.Verify(v => v.Debug($"Transaction complete. Removed and published successfully. EventId: {CqrsConstants.Event1Id}"));
			cqrsRepositoryLogger.Verify(v => v.Debug($"Published event. EventId: {CqrsConstants.Event1Id}"));
			cqrsRepositoryLogger.Verify(v => v.Debug($"Published event. EventId: {CqrsConstants.Event2Id}"), Times.Never);
		}

		public void Dispose()
		{
		}
	}
}
