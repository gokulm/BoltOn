using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cache;
using BoltOn.Logging;
using BoltOn.Requestor.Pipeline;
using BoltOn.Tests.Cache.Fakes;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Cache
{
	public class CacheResponseInterceptorTests
	{
		[Fact]
		public async Task RunAsync_RequestThatDoesNotImplementICacheResponse_CallsNextDelegate()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var logger = autoMocker.GetMock<IBoltOnLogger<CacheResponseInterceptor>>();

			var boltOnCache = autoMocker.GetMock<IBoltOnCache>();
			Func<IRequest<string>, CancellationToken, Task<string>> nextDelegate =
				(r, c) => new Mock<IHandler<IRequest<string>, string>>().Object.HandleAsync(r, c);
			var sut = autoMocker.CreateInstance<CacheResponseInterceptor>();

			// act
			await sut.RunAsync(new Mock<IRequest<string>>().Object, default, nextDelegate);

			// assert
			logger.Verify(l => l.Debug("CacheResponseInterceptor started"), Times.Never);
		}

		[Fact]
		public async Task RunAsync_ResponseInCache_ReturnsResponse()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var logger = autoMocker.GetMock<IBoltOnLogger<CacheResponseInterceptor>>();

			var boltOnCache = autoMocker.GetMock<IBoltOnCache>();
			boltOnCache.Setup(b => b.GetAsync<string>("TestKey", It.IsAny<CancellationToken>(),
				null, It.IsAny<TimeSpan?>())).Returns(Task.FromResult("TestValue"));
			Func<TestRequest, CancellationToken, Task<string>> nextDelegate =
				(r, c) => new Mock<IHandler<TestRequest, string>>().Object.HandleAsync(r, c);
			var sut = autoMocker.CreateInstance<CacheResponseInterceptor>();

			// act
			await sut.RunAsync(new TestRequest(), default, nextDelegate);

			// assert
			logger.Verify(l => l.Debug("CacheResponseInterceptor started"));
			logger.Verify(l => l.Debug("Retrieving response from cache. Key: TestKey"));
			logger.Verify(l => l.Debug("Returning response from cache"));
			boltOnCache.Verify(b => b.GetAsync<string>("TestKey", It.IsAny<CancellationToken>(),
				null, It.IsAny<TimeSpan?>()));
		}
	}
}
