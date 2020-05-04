using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Caching;
using BoltOn.Logging;
using BoltOn.Tests.Caching.Fakes;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.Caching
{
	public class BoltOnCacheTests
	{
		[Fact]
		public async Task SetAsync_SetString_SetsStringInTheCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = new Mock<IDistributedCache>();
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
			var sut = new BoltOnCache(distributedCache.Object, logger.Object);

			// act
			await sut.SetAsync("TestKey", "TestValue");

			// assert
			logger.Verify(l => l.Debug("Setting value in cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.SetAsync("TestKey", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SetAsync_SetObject_SetsObjectInTheCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = new Mock<IDistributedCache>();
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
			var sut = new BoltOnCache(distributedCache.Object, logger.Object);

			// act
			await sut.SetAsync("StudentId", new Student());

			// assert
			logger.Verify(l => l.Debug("Setting value in cache... Key: StudentId"));
			distributedCache.Verify(l =>
				l.SetAsync("StudentId", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetAsync_GetString_GetsExpectedStringFromTheCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = new Mock<IDistributedCache>();
			distributedCache.Setup(s => s.GetAsync("TestKey", It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(CacheTestHelper.ToByteArray("TestValue")));
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
			var sut = new BoltOnCache(distributedCache.Object, logger.Object);

			// act
			var result = await sut.GetAsync<string>("TestKey");

			// assert
			Assert.Equal("TestValue", result);
			logger.Verify(l => l.Debug("Getting from cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.GetAsync("TestKey", It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetAsync_EmptyCacheButWithValueGetter_InvokesValueGetterAndSetsCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = new Mock<IDistributedCache>();
			distributedCache.Setup(s => s.GetAsync("TestKey", It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(CacheTestHelper.ToByteArray(null)));
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
			var valueGetter = new Func<Task<string>>(() => Task.FromResult("TestValue"));
			var sut = new BoltOnCache(distributedCache.Object, logger.Object);

			// act
			var result = await sut.GetAsync<string>("TestKey", valueGetter: valueGetter);

			// assert
			Assert.Equal("TestValue", result);
			logger.Verify(l => l.Debug("Getting from cache... Key: TestKey"));
			logger.Verify(l => l.Debug("Invoking valueGetter..."));
			distributedCache.Verify(l =>
				l.GetAsync("TestKey", It.IsAny<CancellationToken>()), Times.Once);
			logger.Verify(l => l.Debug("Setting value in cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.SetAsync("TestKey", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetAsync_NonEmptyCacheWithSlidingExpiration_SlidesCacheValue()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = new Mock<IDistributedCache>();
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
			var sut = new BoltOnCache(distributedCache.Object, logger.Object);

			// act
			var result = await sut.GetAsync<string>("TestKey", slidingExpiration: TimeSpan.FromSeconds(2));

			// assert
			logger.Verify(l => l.Debug("Getting from cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.GetAsync("TestKey", It.IsAny<CancellationToken>()), Times.Once);
			logger.Verify(l => l.Debug("Setting value in cache... Key: TestKey"));
			logger.Verify(l => l.Debug("Sliding cache expiration..."));
			distributedCache.Verify(l =>
				l.SetAsync("TestKey", It.IsAny<byte[]>(),
				It.Is<DistributedCacheEntryOptions>(d => d.SlidingExpiration == TimeSpan.FromSeconds(2)),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task RemoveAsync_Key_RemovesValueFromCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = new Mock<IDistributedCache>();
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
			var sut = new BoltOnCache(distributedCache.Object, logger.Object);

			// act
			await sut.RemoveAsync("TestKey");

			// assert
			logger.Verify(l => l.Debug("Removing from cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.RemoveAsync("TestKey", It.IsAny<CancellationToken>()), Times.Once);
		}

		
	}
}
