﻿using BoltOn.Requestor;
using BoltOn.Tests.Requestor.Fakes;
using Moq;
using Moq.AutoMock;
using System;
using System.Threading;
using System.Threading.Tasks;
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
            var sut = autoMocker.CreateInstance<BoltOn.Requestor.Requestor>();
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
