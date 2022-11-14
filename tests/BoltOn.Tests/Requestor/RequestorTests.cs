using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Logging;
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
            serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestRequest, bool>)))
                          .Returns(testHandler.Object);
            var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
            var request = new TestRequest();
            testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // act
            var result = await sut.ProcessAsync(request);

            // assert 
            Assert.True(result);
        }

        public void Dispose()
        {
        }
    }
}
