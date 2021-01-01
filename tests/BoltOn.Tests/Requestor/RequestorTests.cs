using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BoltOn.Data.EF;
using BoltOn.Logging;
using BoltOn.Requestor.Interceptors;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Requestor.Fakes;
using BoltOn.UoW;
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
			testHandler.Setup( s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

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
			var logger = new Mock<IBoltOnLogger<TestInterceptor>>();
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
			var logger = new Mock<IBoltOnLogger<TestRequestSpecificInterceptor>>();
			var logger2 = new Mock<IBoltOnLogger<StopwatchInterceptor>>();
			var boltOnClock = new Mock<IBoltOnClock>();
			var currentDateTime = DateTime.Now;
			boltOnClock.Setup(s => s.Now).Returns(currentDateTime);
			serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestRequest, bool>)))
				.Returns(testHandler.Object);
			autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor> { new TestRequestSpecificInterceptor(logger.Object),
				new StopwatchInterceptor(logger2.Object, boltOnClock.Object) });
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
		}

		[Fact]
		public async Task Process_RequestorWithCommandRequest_ExecutesUoWInterceptorAndStartsTransactionsWithDefaultCommandIsolationLevel()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var logger = new Mock<IBoltOnLogger<UnitOfWorkInterceptor>>();
			serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestCommand, bool>)))
				.Returns(testHandler.Object);
			var request = new TestCommand();
			autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>
			{
				new UnitOfWorkInterceptor(logger.Object)
			});
			var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
			testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

			// act
			var result = await sut.ProcessAsync(request);

			// assert 
			Assert.True(result);
			logger.Verify(l => l.Debug("UnitOfWorkInterceptor ended"));
   		}

		[Fact]
		public async Task Process_RequestorWithCommandRequestAndHandlerThrowsException_ExecutesUoWInterceptorAndStartsTransactionsButNotCommit()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var logger = new Mock<IBoltOnLogger<UnitOfWorkInterceptor>>();
			serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestCommand, bool>)))
				.Returns(testHandler.Object);
			var request = new TestCommand();
			autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>
			{
				new UnitOfWorkInterceptor(logger.Object)
			});
			var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
			testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Throws<Exception>();

			// act & assert 
			await Assert.ThrowsAsync<Exception>(() => sut.ProcessAsync(request));
			logger.Verify(l => l.Debug("UnitOfWorkInterceptor ended"), Times.Never);
		}

		[Fact]
		public async Task Process_RequestorWithAsyncHandlerThrowsException_ExecutesUoWInterceptorAndStartsTransactionsButNotCommit()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			var logger = new Mock<IBoltOnLogger<UnitOfWorkInterceptor>>();
			serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestCommand, bool>)))
				.Returns(testHandler.Object);
			var request = new TestCommand();
			autoMocker.Use<IEnumerable<IInterceptor>>(new List<IInterceptor>
			{
				new UnitOfWorkInterceptor(logger.Object)
			});
			var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
			testHandler.Setup(s => s.HandleAsync(request, default)).Throws<Exception>();

            // act 
		    var result = await Record.ExceptionAsync(async () => await sut.ProcessAsync(request));

            //assert 
            Assert.NotNull(result);
            logger.Verify(l => l.Debug("UnitOfWorkInterceptor ended"), Times.Never);
		}

		[Fact]
		public async Task Process_RegisteredHandlerThatThrowsException_ReturnsUnsuccessfulResult()
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
			testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Throws(new Exception("handler failed"));

			// act
			var result = await Record.ExceptionAsync(async () => await sut.ProcessAsync(request));

			// assert 
			Assert.NotNull(result);
			Assert.Equal("handler failed", result.Message);
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

		[Fact]
		public async Task Get_RequestorWithQueryRequest_ExecutesEFQueryTrackingBehaviorInterceptorAndDisablesTracking()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestQuery, bool>)))
				.Returns(testHandler.Object);
			var request = new TestQuery();
			var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
			testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

			// act
			var result = await sut.ProcessAsync(request);

			// assert 
			Assert.True(result);
		}

		[Fact]
		public async Task Get_RequestorWithQueryUncommittedRequest_ExecutesChangeTrackerContextInterceptorAndDisablesTracking()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestStaleQuery, bool>)))
				.Returns(testHandler.Object);
			var request = new TestStaleQuery();
			var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
			testHandler.Setup(s => s.HandleAsync(request, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

			// act
			var result = await sut.ProcessAsync(request);

			// assert 
			Assert.True(result);
		}

		[Fact]
		public async Task Get_RequestorWithCommandRequest_ExecutesChangeTrackerContextInterceptorAndEnablesTracking()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var serviceProvider = autoMocker.GetMock<IServiceProvider>();
			var testHandler = new Mock<TestHandler>();
			serviceProvider.Setup(s => s.GetService(typeof(IHandler<TestCommand, bool>)))
				.Returns(testHandler.Object);
			var request = new TestCommand();
			var sut = autoMocker.CreateInstance<BoltOn.Requestor.Pipeline.Requestor>();
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
