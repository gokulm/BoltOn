using System.Threading.Tasks;
using BoltOn.Caching;
using BoltOn.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoltOn.Tests.Caching
{
	public class BoltOnCacheIntegrationTests
	{
		[Fact]
		public async Task SetAsync_SetNull_ThrowsException()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnCaching();
			});
			serviceCollection.AddDistributedMemoryCache();

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var sut = serviceProvider.GetService<IBoltOnCache>();

			// act
			var result = await Record.ExceptionAsync(() => sut.SetAsync<string>("TestKey", null));

			// assert
			Assert.NotNull(result);
			Assert.Equal("Value cannot be null. (Parameter 'value')", result.Message);
		}

		[Fact]
		public async Task SetAsync_SetString_SetsStringInTheCache()
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

			// act
			await sut.SetAsync("TestKey", "TestValue");

			// assert
			Assert.Equal("TestValue", await sut.GetAsync<string>("TestKey"));
			logger.Verify(l => l.Debug("Setting value in cache... Key: TestKey"));
		}
	}
}
