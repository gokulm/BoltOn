using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Caching;
using BoltOn.Logging;
using BoltOn.Tests.Caching.Fakes;
using BoltOn.Tests.Other;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoltOn.Tests.Caching
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
				b.BoltOnCaching();
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
		}

		[Fact]
		public async Task SetAndGetAsync_SetAndGetString_SetsStringAndGetReturnsExpectedString()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnCaching();
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

			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
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
		}

		[Fact]
		public async Task SetAndGetAsync_SetAndGetObject_SetsObjectAndReturnsExpectedObject()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnCaching();
			});
			serviceCollection.AddDistributedMemoryCache();
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
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
		}

		[Fact]
		public async Task SetAsync_SetObjectAndGetDifferentObjectWithSameSetOfProperties_ReturnsObject()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnCaching();
			});
			serviceCollection.AddDistributedMemoryCache();
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
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
		}

		[Fact]
		public async Task SetAndGet_SetWithExpiryIntervalAndWhenRetrievedBeforeAndAfterExpiryInterval_IsSuccessful()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnCaching();
			});
			serviceCollection.AddDistributedMemoryCache();
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
			serviceCollection.AddTransient(s => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();
			var student = new Fakes.Student { FirstName = "first", Id = Guid.NewGuid() };
			const int expirationInterval = 500;

			// act
			await sut.SetAsync("StudentId", student, slidingExpiration: TimeSpan.FromMilliseconds(expirationInterval));
			var result = await sut.GetAsync<Fakes.Student>("StudentId");

			// assert
			Assert.NotNull(result);
			Assert.Equal(student.Id, result.Id);
			logger.Verify(l => l.Debug("Setting value in cache... Key: StudentId"));
			Thread.Sleep(expirationInterval + 200);
			var result2 = await sut.GetAsync<Fakes.Student>("StudentId");
			Assert.Null(result2);
		}
	}
}
