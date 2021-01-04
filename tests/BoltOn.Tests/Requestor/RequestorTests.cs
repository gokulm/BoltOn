using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Requestor.Fakes;
using BoltOn.Transaction;
using BoltOn.Utilities;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Requestor
{
    public class RequestorTests : IDisposable
    {
        [Fact]
        public async Task Process_RegisteredHandlerThatReturnsBool_ReturnsSuccessfulResult()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var serviceProvider = autoMocker.GetMock<IServiceProvider>();
            var testHandler = new Mock<TestHandler>();
            serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestRequest, bool>)))
                          .Returns(testHandler.Object);
            autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>());
            var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
            var request = new TestRequest();
            testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // act
            var result = await sut.ProcessAsync(request);

            // assert 
            Assert.True(result);
        }

        [Fact]
        public async Task Process_RequestorWithInterceptor_ExecutesInterceptor()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var serviceProvider = autoMocker.GetMock<IServiceProvider>();
            var testHandler = new Mock<TestHandler>();
            var logger = new Mock<IAppLogger<TestInterceptor>>();
            serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestRequest, bool>)))
                          .Returns(testHandler.Object);
            autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor> { new TestInterceptor(logger.Object) });
            var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
            var request = new TestRequest();
            testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // act
            var result = await sut.ProcessAsync(request);

            // assert 
            Assert.True(result);
            logger.Verify(l => l.Debug("TestInterceptor Started"));
            logger.Verify(l => l.Debug("TestInterceptor Ended"));
        }

        [Fact]
        public async Task Process_RequestorWithRequestSpecificInterceptor_ExecutesInterceptor()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var serviceProvider = autoMocker.GetMock<IServiceProvider>();
            var testHandler = new Mock<TestHandler>();
            var logger = new Mock<IAppLogger<TestRequestSpecificInterceptor>>();
            var logger2 = new Mock<IAppLogger<StopwatchInterceptor>>();
            var logger3 = new Mock<IAppLogger<TransactionInterceptor>>();
            var appClock = new Mock<IAppClock>();
            var currentDateTime = DateTime.Now;
            appClock.Setup(s => s.Now).Returns(currentDateTime);
            serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestRequest, bool>)))
                .Returns(testHandler.Object);
            autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor> { new TestRequestSpecificInterceptor(logger.Object),
                new StopwatchInterceptor(logger2.Object, appClock.Object) ,
                new TransactionInterceptor(logger3.Object) });
            var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
            var request = new TestRequest();
            testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // act
            var result = await sut.ProcessAsync(request);

            // assert 
            Assert.True(result);
            logger.Verify(l => l.Debug("TestRequestSpecificInterceptor Started"), Times.Never);
            logger.Verify(l => l.Debug("TestRequestSpecificInterceptor Ended"), Times.Never);
            logger2.Verify(l => l.Debug($"StopwatchInterceptor started at {currentDateTime}"), Times.Once);
            logger3.Verify(l => l.Debug("Request didn't enable transaction"));
        }

        [Fact]
        public async Task Process_RequestorWithReadCommittedRequest_ExecutesTransactionInterceptorAndStartsTransactionsWithAppropriateIsolationLevel()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var serviceProvider = autoMocker.GetMock<IServiceProvider>();
            var testHandler = new Mock<TestHandler>();
            var logger = new Mock<IAppLogger<TransactionInterceptor>>();
            serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestCommand, bool>)))
                .Returns(testHandler.Object);
            var request = new TestCommand();
            autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>
            {
                new TransactionInterceptor(logger.Object)
            });
            var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
            testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // act
            var result = await sut.ProcessAsync(request);

            // assert 
            Assert.True(result);
            logger.Verify(l => l.Debug($"About to start transaction. TransactionScopeOption: {TransactionScopeOption.RequiresNew} " +
                    $"IsolationLevel: {IsolationLevel.ReadCommitted} Timeout: { TransactionManager.DefaultTimeout}"));
            logger.Verify(l => l.Debug("Transaction completed"));
        }

        [Fact]
        public async Task Process_RequestorWithAsyncHandlerThrowsException_ExecutesTransactionInterceptorAndStartsTransactionsButNotCommit()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var serviceProvider = autoMocker.GetMock<IServiceProvider>();
            var testHandler = new Mock<TestHandler>();
            var logger = new Mock<IAppLogger<TransactionInterceptor>>();
            serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestCommand, bool>)))
                .Returns(testHandler.Object);
            var request = new TestCommand();
            autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>
            {
                new TransactionInterceptor(logger.Object)
            });
            var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
            testHandler.Setup(s => s.HandleAsync(request, default)).Throws<Exception>();

            // act 
            var result = await Record.ExceptionAsync(async () => await sut.ProcessAsync(request));

            //assert 
            Assert.NotNull(result);
            logger.Verify(l => l.Debug("Transaction completed"), Times.Never);
        }

        [Fact]
        public async Task Process_RegisteredAsyncHandlerThatThrowsException_ReturnsUnsuccessfulResult()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var serviceProvider = autoMocker.GetMock<IServiceProvider>();
            var testHandler = new Mock<TestHandler>();
            serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestRequest, bool>)))
                           .Returns(testHandler.Object);
            autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>());
            var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
            var request = new TestRequest();
            testHandler.Setup(s => s.HandleAsync(request, default)).Throws(new Exception("handler failed"));

            // act
            var result = await Record.ExceptionAsync(() => sut.ProcessAsync(request));

            // assert 
            Assert.NotNull(result);
            Assert.Equal("handler failed", result.Message);
        }

        [Fact]
        public async Task Process_UnregisteredHandler_ReturnsUnsuccessfulResult()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var serviceProvider = autoMocker.GetMock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestRequest, bool>)))
                          .Returns(null);
            autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>());
            var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
            var request = new TestRequest();

            // act
            var result = await Record.ExceptionAsync(() => sut.ProcessAsync(request));

            // assert 
            Assert.NotNull(result);
            Assert.Equal(string.Format("Handler not found for request: {0}", request), result.Message);
        }

        public void Dispose()
        {
        }
    }
}
