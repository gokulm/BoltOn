using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Cqrs.Fakes;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Cqrs
{
    //public class CqrsInterceptorTests
    //{
    //    [Fact]
    //    public async Task RunAsync_PurgeEventsToBeProcessed_BothEventsGetProcessedAndPurged()
    //    {
    //        // arrange
    //        var autoMocker = new AutoMocker();
    //        var successId = Guid.NewGuid();
    //        var successId2 = Guid.NewGuid();
    //        //var eventBag = autoMocker.GetMock<EventBag>();
    //        var removeEventToBeProcessedHandle = new Mock<Func<IDomainEvent, Task>>();
    //        var studentCreatedEvent = new StudentCreatedEvent
    //        {
    //            Id = successId,
    //            EntityType = typeof(Student).Name,
    //            //DestinationTypeName = typeof(Student).Name
    //        };
    //        var studentUpdatedEvent = new StudentUpdatedEvent
    //        {
    //            Id = successId2,
    //            EntityType = typeof(Student).Name,
    //            //DestinationTypeName = typeof(Student).Name
    //        };
    //        var eventsToBeProcessed = new Dictionary<IDomainEvent, Func<IDomainEvent, Task>>
    //        {
    //            { studentCreatedEvent,  removeEventToBeProcessedHandle.Object },
    //            { studentUpdatedEvent,  removeEventToBeProcessedHandle.Object }
    //        };
    //        //eventBag.Setup(s => s.EventsToBeProcessed).Returns(eventsToBeProcessed);

    //        var logger = autoMocker.GetMock<IAppLogger<CqrsInterceptor>>();
    //        autoMocker.GetMock<IEventDispatcher>();

    //        var cqrsOptions = autoMocker.GetMock<CqrsOptions>();
    //        cqrsOptions.Setup(s => s.PurgeEventsToBeProcessed).Returns(true);

    //        Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
    //            (r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
    //        var sut = autoMocker.CreateInstance<CqrsInterceptor>();

    //        // act
    //        await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

    //        // assert
    //        logger.Verify(v => v.Debug("About to dispatch EventsToBeProcessed..."));
    //        logger.Verify(v => v.Debug($"Publishing event. Id: {successId} " +
    //            $"SourceType: {typeof(Student).Name}"));
    //        logger.Verify(v => v.Debug($"Publishing event. Id: {successId2} " +
    //            $"SourceType: {typeof(Student).Name}"));
    //        logger.Verify(v => v.Debug($"Dispatching or purging failed. Event Id: {successId}"), Times.Never);
    //        logger.Verify(v => v.Debug($"Dispatching or purging failed. Event Id: {successId2}"), Times.Never);
    //        //eventBag.Verify(v => v.RemoveEventToBeProcessed(It.IsAny<IDomainEvent>()), Times.Exactly(2));
    //    }

    //    [Fact]
    //    public async Task RunAsync_Failed1stEventsToBeProcessedOutOf2Events_BothEventsDoNotGetProcessed()
    //    {
    //        // arrange
    //        var autoMocker = new AutoMocker();
    //        var failedId = Guid.NewGuid();
    //        var failedId2 = Guid.NewGuid();
    //        //var eventBag = autoMocker.GetMock<EventBag>();
    //        var removeEventToBeProcessedHandle = new Mock<Func<IDomainEvent, Task>>();

    //        var studentCreatedEvent = new StudentCreatedEvent
    //        {
    //            Id = failedId,
    //            EntityType = typeof(Student).Name,
    //            //DestinationTypeName = typeof(Student).Name
    //        };
    //        var studentUpdatedEvent = new StudentUpdatedEvent
    //        {
    //            Id = failedId2,
    //            EntityType = typeof(Student).Name,
    //            //DestinationTypeName = typeof(Student).Name
    //        };
    //        var eventsToBeProcessed = new Dictionary<IDomainEvent, Func<IDomainEvent, Task>>
    //        {
    //            { studentCreatedEvent,  removeEventToBeProcessedHandle.Object },
    //            { studentUpdatedEvent,  removeEventToBeProcessedHandle.Object }
    //        };
    //        //eventBag.Setup(s => s.EventsToBeProcessed).Returns(eventsToBeProcessed);

    //        var logger = autoMocker.GetMock<IAppLogger<CqrsInterceptor>>();
    //        var eventDispatcher = autoMocker.GetMock<IEventDispatcher>();
    //        eventDispatcher.Setup(d => d.DispatchAsync(It.Is<IDomainEvent>(t => t.Id == failedId), default))
    //            .Throws(new Exception());

    //        var cqrsOptions = autoMocker.GetMock<CqrsOptions>();
    //        cqrsOptions.Setup(s => s.PurgeEventsToBeProcessed).Returns(true);

    //        Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
    //            (r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
    //        var sut = autoMocker.CreateInstance<CqrsInterceptor>();

    //        // act
    //        await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

    //        // assert
    //        logger.Verify(v => v.Debug("About to dispatch EventsToBeProcessed..."));
    //        logger.Verify(v => v.Debug($"Publishing event. Id: {failedId} " +
    //            $"SourceType: {typeof(Student).Name}"));
    //        logger.Verify(v => v.Error($"Dispatching or purging failed. Event Id: {failedId}"));
    //        logger.Verify(v => v.Debug($"Publishing event. Id: {failedId2} " +
    //            $"SourceType: {typeof(Student).Name}"), Times.Never);
    //        //Assert.True(eventBag.Object.EventsToBeProcessed.Count == 2);
    //    }

    //    [Fact]
    //    public async Task RunAsync_Failed2ndEventToBeProcessedOutOf2Events_2ndEventDoesNotGetProcessed()
    //    {
    //        // arrange
    //        var autoMocker = new AutoMocker();
    //        var failedId = Guid.NewGuid();
    //        var failedId2 = Guid.NewGuid();
    //        //var eventBag = autoMocker.GetMock<EventBag>();
    //        var removeEventToBeProcessedHandle = new Mock<Func<IDomainEvent, Task>>();
    //        var studentCreatedEvent = new StudentCreatedEvent
    //        {
    //            Id = failedId,
    //            EntityType = typeof(Student).Name,
    //            //DestinationTypeName = typeof(Student).Name
    //        };
    //        var studentUpdatedEvent = new StudentUpdatedEvent
    //        {
    //            Id = failedId2,
    //            EntityType = typeof(Student).Name,
    //            //DestinationTypeName = typeof(Student).Name
    //        };
    //        var eventsToBeProcessed = new Dictionary<IDomainEvent, Func<IDomainEvent, Task>>
    //        {
    //            { studentCreatedEvent,  removeEventToBeProcessedHandle.Object },
    //            { studentUpdatedEvent,  removeEventToBeProcessedHandle.Object }
    //        };
    //        //eventBag.Setup(s => s.EventsToBeProcessed).Returns(eventsToBeProcessed);
    //        var logger = autoMocker.GetMock<IAppLogger<CqrsInterceptor>>();
    //        var eventDispatcher = autoMocker.GetMock<IEventDispatcher>();
    //        eventDispatcher.Setup(d => d.DispatchAsync(It.Is<IDomainEvent>(t => t.Id == failedId2), default))
    //            .Throws(new Exception());

    //        var cqrsOptions = autoMocker.GetMock<CqrsOptions>();
    //        cqrsOptions.Setup(s => s.PurgeEventsToBeProcessed).Returns(true);

    //        Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
    //            (r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
    //        var sut = autoMocker.CreateInstance<CqrsInterceptor>();

    //        // act
    //        await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

    //        // assert
    //        logger.Verify(v => v.Debug("About to dispatch EventsToBeProcessed..."));
    //        logger.Verify(v => v.Debug($"Publishing event. Id: {failedId} " +
    //            $"SourceType: {typeof(Student).Name}"));
    //        logger.Verify(v => v.Error($"Dispatching or purging failed. Event Id: {failedId}"), Times.Never);
    //        logger.Verify(v => v.Debug($"Publishing event. Id: {failedId2} " +
    //            $"SourceType: {typeof(Student).Name}"));
    //        logger.Verify(v => v.Error($"Dispatching or purging failed. Event Id: {failedId2}"));
    //        //eventBag.Verify(v => v.RemoveEventToBeProcessed(It.IsAny<IDomainEvent>()), Times.Once);
    //    }

    //    [Fact]
    //    public async Task RunAsync_PurgeEventsToBeProcessedNotEnabled_EventsToBeProcessedDoNotGetPurged()
    //    {
    //        // arrange
    //        var autoMocker = new AutoMocker();
    //        var successId = Guid.NewGuid();
    //        //var eventBag = autoMocker.GetMock<EventBag>();
    //        var removeEventToBeProcessedHandle = new Mock<Func<IDomainEvent, Task>>();
    //        var studentCreatedEvent = new StudentCreatedEvent
    //        {
    //            Id = successId,
    //            EntityType = typeof(Student).Name,
    //            //DestinationTypeName = typeof(Student).Name
    //        };
    //        var eventsToBeProcessed = new Dictionary<IDomainEvent, Func<IDomainEvent, Task>>
    //        {
    //            { studentCreatedEvent,  removeEventToBeProcessedHandle.Object },
    //        };
    //        //eventBag.Setup(s => s.EventsToBeProcessed).Returns(eventsToBeProcessed);
    //        var logger = autoMocker.GetMock<IAppLogger<CqrsInterceptor>>();

    //        var cqrsOptions = autoMocker.GetMock<CqrsOptions>();
    //        cqrsOptions.Setup(s => s.PurgeEventsToBeProcessed).Returns(false);

    //        Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
    //            (r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
    //        var sut = autoMocker.CreateInstance<CqrsInterceptor>();

    //        // act
    //        await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

    //        // assert
    //        logger.Verify(v => v.Debug("About to dispatch EventsToBeProcessed..."));
    //        logger.Verify(v => v.Debug($"Publishing event. Id: {successId} " +
    //            $"SourceType: {typeof(Student).Name}"));
    //        logger.Verify(v => v.Debug(It.Is<string>(s => s.StartsWith("Removing event. Id: "))), Times.Never);
    //        //eventBag.Verify(v => v.RemoveEventToBeProcessed(It.IsAny<IDomainEvent>()), Times.Once);
    //    }
    //}
}
