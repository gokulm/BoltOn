using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Cqrs.Fakes;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Cqrs
{
    public class CqrsInterceptorTests : IDisposable
    {
		[Fact]
		public async Task RunAsync_PurgeEventsToBeProcessed_BothEventsGetProcessed()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var successId = Guid.NewGuid();
			var successId2 = Guid.NewGuid();
			var eventBag = autoMocker.GetMock<EventBag>();
			eventBag.Setup(s => s.EventsToBeProcessed)
				.Returns(new List<ICqrsEvent>
				{
					new StudentCreatedEvent
					{
						Id = successId,
						SourceTypeName = typeof(Student).Name,
						DestinationTypeName = typeof(Student).Name
					},
					new StudentUpdatedEvent
					{
						Id = successId2,
						SourceTypeName = typeof(Student).Name,
						DestinationTypeName = typeof(Student).Name
					},
				});
			var logger = autoMocker.GetMock<IBoltOnLogger<CqrsInterceptor>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
				.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			logger.Setup(s => s.Error(It.IsAny<string>()))
				.Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
			autoMocker.GetMock<IEventDispatcher>();

			var cqrsOptions = autoMocker.GetMock<CqrsOptions>();
			cqrsOptions.Setup(s => s.PurgeEventsToBeProcessed).Returns(true);

			Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
				(r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
			var sut = autoMocker.CreateInstance<CqrsInterceptor>();

			// act
			await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

			// assert
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										"About to dispatch EventsToBeProcessed..."));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Publishing event. Id: {successId} " +
				$"SourceType: {typeof(Student).Name}"));
			Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Publishing event. Id: {successId2} " +
				$"SourceType: {typeof(Student).Name}"));
			Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Dispatching failed. Id: {successId}"));
			Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Dispatching failed. Id: {successId2}"));
			Assert.True(eventBag.Object.EventsToBeProcessed.Count == 0);
		}

		[Fact]
        public async Task RunAsync_Failed1stEventsToBeProcessedOutOf2Events_BothEventsDoNotGetProcessed()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var failedId = Guid.NewGuid();
            var failedId2 = Guid.NewGuid();
            var eventBag = autoMocker.GetMock<EventBag>();
            eventBag.Setup(s => s.EventsToBeProcessed)
                .Returns(new List<ICqrsEvent>
                {
                    new StudentCreatedEvent
                    {
                        Id = failedId,
                        SourceTypeName = typeof(Student).Name,
                        DestinationTypeName = typeof(Student).Name
                    },
                    new StudentUpdatedEvent
                    {
                        Id = failedId2,
                        SourceTypeName = typeof(Student).Name,
                        DestinationTypeName = typeof(Student).Name
                    },
                });
            var logger = autoMocker.GetMock<IBoltOnLogger<CqrsInterceptor>>();
            logger.Setup(s => s.Debug(It.IsAny<string>()))
                .Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
            logger.Setup(s => s.Error(It.IsAny<string>()))
                .Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
            var eventDispatcher = autoMocker.GetMock<IEventDispatcher>();
            eventDispatcher.Setup(d => d.DispatchAsync(It.Is<ICqrsEvent>(t => t.Id == failedId), default))
                .Throws(new Exception());

            var cqrsOptions = autoMocker.GetMock<CqrsOptions>();
            cqrsOptions.Setup(s => s.PurgeEventsToBeProcessed).Returns(true);

            Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
                (r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
            var sut = autoMocker.CreateInstance<CqrsInterceptor>();

            // act
            await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

            // assert
            Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
                                        "About to dispatch EventsToBeProcessed..."));
            Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Publishing event. Id: {failedId} " +
                $"SourceType: {typeof(Student).Name}"));
            Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Dispatching failed. Id: {failedId}"));
            Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Publishing event. Id: {failedId2} " +
                    $"SourceType: {typeof(Student).Name}"));
            Assert.True(eventBag.Object.EventsToBeProcessed.Count == 2);
        }

        [Fact]
        public async Task RunAsync_Failed2ndEventToBeProcessedOutOf2Events_2ndEventDoesNotGetProcessed()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var failedId = Guid.NewGuid();
            var failedId2 = Guid.NewGuid();
            var eventBag = autoMocker.GetMock<EventBag>();
            eventBag.Setup(s => s.EventsToBeProcessed)
                .Returns(new List<ICqrsEvent>
                {
                    new StudentCreatedEvent
                    {
                        Id = failedId,
                        SourceTypeName = typeof(Student).Name,
                        DestinationTypeName = typeof(Student).Name
                    },
                    new StudentUpdatedEvent
                    {
                        Id = failedId2,
                        SourceTypeName = typeof(Student).Name,
                        DestinationTypeName = typeof(Student).Name
                    },
                });
            var logger = autoMocker.GetMock<IBoltOnLogger<CqrsInterceptor>>();
            logger.Setup(s => s.Debug(It.IsAny<string>()))
                .Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
            logger.Setup(s => s.Error(It.IsAny<string>()))
                .Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
            var eventDispatcher = autoMocker.GetMock<IEventDispatcher>();
            eventDispatcher.Setup(d => d.DispatchAsync(It.Is<ICqrsEvent>(t => t.Id == failedId2), default))
                .Throws(new Exception());

            var cqrsOptions = autoMocker.GetMock<CqrsOptions>();
            cqrsOptions.Setup(s => s.PurgeEventsToBeProcessed).Returns(true);

            Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
                (r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
            var sut = autoMocker.CreateInstance<CqrsInterceptor>();

            // act
            await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

            // assert
            Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
                                        "About to dispatch EventsToBeProcessed..."));
            Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Publishing event. Id: {failedId} " +
                $"SourceType: {typeof(Student).Name}"));
            Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Dispatching failed. Id: {failedId}"));
            Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Publishing event. Id: {failedId2} " +
                    $"SourceType: {typeof(Student).Name}"));
            Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Dispatching failed. Id: {failedId2}"));
            Assert.True(eventBag.Object.EventsToBeProcessed.Count == 1);
        }

        [Fact]
        public async Task RunAsync_PurgeEventsToBeProcessedNotEnabled_EventsToBeProcessedDoNotGetPurged()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var failedId = Guid.NewGuid();
            var failedId2 = Guid.NewGuid();
            var eventBag = autoMocker.GetMock<EventBag>();
            eventBag.Setup(s => s.ProcessedEvents)
                .Returns(new List<ICqrsEvent>
                {
                    new StudentCreatedEvent
                    {
                        Id = failedId,
                        SourceTypeName = typeof(Student).Name,
                        DestinationTypeName = typeof(Student).Name
                    },
                    new StudentUpdatedEvent
                    {
                        Id = failedId2,
                        SourceTypeName = typeof(Student).Name,
                        DestinationTypeName = typeof(Student).Name
                    },
                });
            var logger = autoMocker.GetMock<IBoltOnLogger<CqrsInterceptor>>();
            logger.Setup(s => s.Debug(It.IsAny<string>()))
                .Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));

            var cqrsOptions = autoMocker.GetMock<CqrsOptions>();
            cqrsOptions.Setup(s => s.PurgeEventsToBeProcessed).Returns(false);

            Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
                (r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
            var sut = autoMocker.CreateInstance<CqrsInterceptor>();

            // act
            await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

            // assert
            Assert.NotNull(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
                                        "About to dispatch EventsToBeProcessed..."));
            Assert.Null(CqrsTestHelper.LoggerStatements.FirstOrDefault(f => f ==
                                        "About to dispatch ProcessedEvents..."));
            Assert.True(eventBag.Object.ProcessedEvents.Count == 2);
        }

        public void Dispose()
        {
            CqrsTestHelper.LoggerStatements.Clear();
        }
    }
}
