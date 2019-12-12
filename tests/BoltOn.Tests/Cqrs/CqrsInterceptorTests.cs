using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Cqrs
{
    public class CqrsInterceptorTests : IDisposable
    {
        [Fact]
        public async  Task RunAsync_Failed1stProcessedEventOutOf2Events_2ndEventDoesNotGetRemoved()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var failedId = Guid.NewGuid();
            var failedId2 = Guid.NewGuid();
            var eventBag = autoMocker.GetMock<EventBag>();
            eventBag.Setup(s => s.ProcessedEvents)
                .Returns(new List<ICqrsEvent>()
                {
                    new StudentCreatedEvent {Id = failedId},
                    new StudentUpdatedEvent {Id = failedId2},
                });
            var logger = autoMocker.GetMock<IBoltOnLogger<CqrsInterceptor>>();
            logger.Setup(s => s.Debug(It.IsAny<string>()))
                .Callback<string>(st => CqrsTestHelper.LoggerStatements.Add(st));
            var eventDispatcher = autoMocker.GetMock<IEventDispatcher>();
            eventDispatcher.Setup(d => d.DispatchAsync(It.Is<CqrsEventProcessedEvent>(t => t.Id == failedId), default))
                .Throws(new Exception());

            var cqrsOptions = autoMocker.GetMock<CqrsOptions>();
            cqrsOptions.Setup(s => s.ClearEventsEnabled).Returns(true);

            Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
                (r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
            var sut = autoMocker.CreateInstance<CqrsInterceptor>();

            // act
            await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

            // assert

        }

        public void Dispose()
        {
        }
    }

    public class TestInterceptorRequest : IRequest<string>
    {
    }

    public class TestInterceptorRequestHandler : IHandler<TestInterceptorRequest, string>
    {
        public Task<string> HandleAsync(TestInterceptorRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
