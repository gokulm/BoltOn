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
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var serializer = autoMocker.GetMock<IBoltOnCacheSerializer>();
			var sut = autoMocker.CreateInstance<BoltOnCache>();

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
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var serializer = autoMocker.GetMock<IBoltOnCacheSerializer>();
			var sut = autoMocker.CreateInstance<BoltOnCache>();

			// act
			await sut.SetAsync("StudentId", new Student());

			// assert
			logger.Verify(l => l.Debug("Setting value in cache... Key: StudentId"));
			distributedCache.Verify(l =>
				l.SetAsync("StudentId", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task SetAsync_SetNull_DoesNotCallUnderlyingDistributedCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var serializer = autoMocker.GetMock<IBoltOnCacheSerializer>();
			var sut = autoMocker.CreateInstance<BoltOnCache>();

			// act
			await sut.SetAsync<string>("TestKey", null);

			// assert
			logger.Verify(l => l.Debug("Setting value in cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.SetAsync("TestKey", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
				It.IsAny<CancellationToken>()), Times.Never);
		}

		[Fact]
		public async Task GetAsync_GetString_GetsExpectedStringFromTheCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var serializer = autoMocker.GetMock<IBoltOnCacheSerializer>();
			serializer.Setup(s => s.FromByteArray<string>(It.IsAny<byte[]>())).Returns("TestValue");
			var sut = autoMocker.CreateInstance<BoltOnCache>();

			// act
			var result = await sut.GetAsync<string>("TestKey");

			// assert
			Assert.Equal("TestValue", result);
			logger.Verify(l => l.Debug("Getting from cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.GetAsync("TestKey", It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GetAsync_GetObject_GetsExpectedObjectFromTheCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var serializer = autoMocker.GetMock<IBoltOnCacheSerializer>();
			var student = new Student();
			serializer.Setup(s => s.FromByteArray<Student>(It.IsAny<byte[]>()))
				.Returns(student);
			var sut = autoMocker.CreateInstance<BoltOnCache>();

			// act
			var result = await sut.GetAsync<Student>("TestKey");

			// assert
			Assert.Equal(student, result);
			logger.Verify(l => l.Debug("Getting from cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.GetAsync("TestKey", It.IsAny<CancellationToken>()), Times.Once);
			logger.Verify(l => l.Debug("Invoking valueGetter..."), Times.Never);
		}

		[Fact]
		public async Task GetAsync_GetStringWithValueInCacheAndValueGetter_GetsExpectedStringFromTheCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var serializer = autoMocker.GetMock<IBoltOnCacheSerializer>();
			serializer.Setup(s => s.FromByteArray<string>(It.IsAny<byte[]>())).Returns("TestValue");
			var valueGetter = new Func<Task<string>>(() => Task.FromResult("value getter string"));
			var sut = autoMocker.CreateInstance<BoltOnCache>();

			// act
			var result = await sut.GetAsync("TestKey", valueGetter: valueGetter);

			// assert
			Assert.Equal("TestValue", result);
			logger.Verify(l => l.Debug("Getting from cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.GetAsync("TestKey", It.IsAny<CancellationToken>()), Times.Once);
			logger.Verify(l => l.Debug("Invoking valueGetter..."), Times.Never);
		}

		[Fact]
		public async Task GetAsync_EmptyCacheButWithValueGetter_InvokesValueGetterAndSetsCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			distributedCache.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult((byte[])null));
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var valueGetter = new Func<Task<string>>(() => Task.FromResult("TestValue"));
			var sut = autoMocker.CreateInstance<BoltOnCache>();

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
		public async Task GetAsync_EmptyCacheButWithValueGetterThrowingException_ThrowsException()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			distributedCache.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult((byte[])null));
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var valueGetter = new Func<Task<string>>(() => { throw new Exception("test exception"); });
			var sut = autoMocker.CreateInstance<BoltOnCache>();

			// act
			var result = await Record.ExceptionAsync(async () =>
				await sut.GetAsync<string>("TestKey", valueGetter: valueGetter));

			// assert
			Assert.NotNull(result);
			Assert.Equal("test exception", result.Message);
			logger.Verify(l => l.Debug("Getting from cache... Key: TestKey"));
			logger.Verify(l => l.Debug("Invoking valueGetter..."));
		}

		[Fact]
		public async Task GetAsync_NonEmptyCacheWithSlidingExpiration_SlidesCacheValue()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var serializer = autoMocker.GetMock<IBoltOnCacheSerializer>();
			serializer.Setup(s => s.FromByteArray<string>(It.IsAny<byte[]>())).Returns("TestValue");
			var sut = autoMocker.CreateInstance<BoltOnCache>();

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
			var distributedCache = autoMocker.GetMock<IDistributedCache>();
			var logger = autoMocker.GetMock<IBoltOnLogger<BoltOnCache>>();
			var serializer = autoMocker.GetMock<IBoltOnCacheSerializer>();
			serializer.Setup(s => s.FromByteArray<string>(It.IsAny<byte[]>())).Returns("TestValue");
			var sut = autoMocker.CreateInstance<BoltOnCache>();

			// act
			await sut.RemoveAsync("TestKey");

			// assert
			logger.Verify(l => l.Debug("Removing from cache... Key: TestKey"));
			distributedCache.Verify(l =>
				l.RemoveAsync("TestKey", It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
