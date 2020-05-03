using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Caching;
using BoltOn.Logging;
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
		public async Task GetAsync_GetString_GetsExpectedStringFromTheCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = new Mock<IDistributedCache>();
			distributedCache.Setup(s => s.GetAsync("TestKey", It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(ToByteArray("TestValue")));
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
				.Returns(Task.FromResult(ToByteArray(null)));
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

		private byte[] ToByteArray(object obj)
		{
			if (obj == null)
			{
				return null;
			}

			var binaryFormatter = new BinaryFormatter();
			using var memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, obj);
			return memoryStream.ToArray();
		}

		private T FromByteArray<T>(byte[] byteArray) where T : class
		{
			if (byteArray == null)
				return default;

			var binaryFormatter = new BinaryFormatter();
			using var memoryStream = new MemoryStream(byteArray);
			return binaryFormatter.Deserialize(memoryStream) as T;
		}
	}
}
