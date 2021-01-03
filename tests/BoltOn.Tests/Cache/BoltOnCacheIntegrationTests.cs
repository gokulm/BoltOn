using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cache;
using BoltOn.Logging;
using BoltOn.Tests.Cache.Fakes;
using BoltOn.Tests.Other;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoltOn.Tests.Cache
{
	public class BoltOnCacheIntegrationTests
	{
		[Fact]
		public async Task SetAsync_SetNull_DoesNotCallUnderlyingDistributedCache()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnCacheModule();
			});

			var distributedCache = new Mock<IDistributedCache>();
			serviceCollection.AddTransient(s => distributedCache.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();

			// act
			await sut.SetAsync<string>("TestKey", null);

			// assert
			distributedCache.Verify(l =>
				l.SetAsync("TestKey", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
				It.IsAny<CancellationToken>()), Times.Never);

			// cleanup
			await sut.RemoveAsync("TestKey");
		}

		[Fact]
		public async Task SetAndGetAsync_SetAndGetString_SetsStringAndGetReturnsExpectedString()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			Setup(serviceCollection);

			var logger = new Mock<IAppLogger<BoltOnCache>>();
			serviceCollection.AddTransient(s => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();

			// act
			await sut.SetAsync("TestKey", "TestValue");
			var result = await sut.GetAsync<string>("TestKey");

			// assert
			Assert.Equal("TestValue", result);
			logger.Verify(l => l.Debug("Setting value in cache... Key: TestKey"));

			// cleanup
			await sut.RemoveAsync("TestKey");
		}

		[Fact]
		public async Task SetAndGetAsync_SetAndGetObject_SetsObjectAndReturnsExpectedObject()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			Setup(serviceCollection);
			var logger = new Mock<IAppLogger<BoltOnCache>>();
			serviceCollection.AddTransient(s => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();
			var student = new Fakes.Student { FirstName = "first", Id = Guid.NewGuid() };

			// act
			await sut.SetAsync("StudentId", student);
			var result = await sut.GetAsync<Fakes.Student>("StudentId");

			// assert
			Assert.NotNull(result);
			Assert.Equal(student.Id, result.Id);
			Assert.Equal(student.FirstName, result.FirstName);
			logger.Verify(l => l.Debug("Setting value in cache... Key: StudentId"));

			// cleanup
			await sut.RemoveAsync("StudentId");
		}

		[Fact]
		public async Task SetAndGetAsync_SetAndGetByteArray_SetsObjectAndReturnsExpectedResult()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			Setup(serviceCollection);
			var logger = new Mock<IAppLogger<BoltOnCache>>();
			serviceCollection.AddTransient(s => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();
			var byteArray = System.Text.Encoding.Default.GetBytes("Test Value");

			// act
			await sut.SetAsync("TestKey", byteArray);
			var result = await sut.GetAsync<byte[]>("TestKey");

			// assert
			Assert.NotNull(result);
			Assert.Equal(byteArray.Length, result.Length);
			logger.Verify(l => l.Debug("Setting value in cache... Key: TestKey"));

			// cleanup
			await sut.RemoveAsync("TestKey");
		}

		[Fact]
		public async Task SetAsync_SetObjectAndGetDifferentObjectWithSameSetOfProperties_ReturnsObject()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			Setup(serviceCollection);
			var logger = new Mock<IAppLogger<BoltOnCache>>();
			serviceCollection.AddTransient(s => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();
			var student = new Fakes.Student { FirstName = "first", Id = Guid.NewGuid() };

			// act
			await sut.SetAsync("StudentId", student);
			var result = await sut.GetAsync<Employee>("StudentId");

			// assert
			Assert.NotNull(result);
			Assert.Equal(student.Id, result.Id);
			Assert.Equal(student.FirstName, result.FirstName);
			logger.Verify(l => l.Debug("Setting value in cache... Key: StudentId"));

			// cleanup
			await sut.RemoveAsync("StudentId");
		}

		[Fact]
		public async Task SetAndGet_SetWithExpiryIntervalAndWhenRetrievedBeforeAndAfterExpiryInterval_IsSuccessful()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			Setup(serviceCollection);
			var logger = new Mock<IAppLogger<BoltOnCache>>();
			serviceCollection.AddTransient(s => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();
			var student = new Fakes.Student { FirstName = "first", Id = Guid.NewGuid() };
			const int expirationInterval = 1000;

			// act
			await sut.SetAsync("Student", student, slidingExpiration: TimeSpan.FromMilliseconds(expirationInterval));
			var result = await sut.GetAsync<Fakes.Student>("Student");

			// assert
			Assert.NotNull(result);
			Assert.Equal(student.Id, result.Id);
			logger.Verify(l => l.Debug("Setting value in cache... Key: Student"));
			await Task.Delay(expirationInterval + 200);
			var result2 = await sut.GetAsync<Fakes.Student>("Student");
			Assert.Null(result2);

			// cleanup
			await sut.RemoveAsync("Student");
		}

		[Fact]
		public async Task SetAndRemoveAndGetAsync_SetAndRemoveAndGetString_SetsRemovesGetsNull()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			Setup(serviceCollection);

			var logger = new Mock<IAppLogger<BoltOnCache>>();
			serviceCollection.AddTransient(s => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();

			// act
			await sut.SetAsync("TestKey", "TestValue");
			await sut.RemoveAsync("TestKey");
			var result = await sut.GetAsync<string>("TestKey");

			// assert
			Assert.Null(result);
			logger.Verify(l => l.Debug("Setting value in cache... Key: TestKey"));
			logger.Verify(l => l.Debug("Removing from cache... Key: TestKey"));
			logger.Verify(l => l.Debug("Getting from cache... Key: TestKey"));

			// cleanup
			await sut.RemoveAsync("TestKey");
		}

		[Fact]
		public async Task RemoveAndGetAsync_RemoveKeyNotInCache_ReturnsNull()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			Setup(serviceCollection);

			var logger = new Mock<IAppLogger<BoltOnCache>>();
			serviceCollection.AddTransient(s => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();

			// act
			await sut.RemoveAsync("TestKey");
			var result = await sut.GetAsync<string>("TestKey");

			// assert
			Assert.Null(result);
			logger.Verify(l => l.Debug("Removing from cache... Key: TestKey"));
			logger.Verify(l => l.Debug("Getting from cache... Key: TestKey"));

			// cleanup
			await sut.RemoveAsync("TestKey");
		}

		private static void Setup(ServiceCollection serviceCollection)
		{
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnCacheModule();
			});

			if (!IntegrationTestHelper.IsRedisCache)
			{
				serviceCollection.AddDistributedMemoryCache();
			}
			else
			{
				serviceCollection.AddStackExchangeRedisCache(options =>
				{
					options.Configuration = "localhost:6379";
				});
			}
		}
	}
}
