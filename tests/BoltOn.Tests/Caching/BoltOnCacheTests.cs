using System;
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
		public async Task SetAsync_SettingString_SetsStringInTheCache()
		{
			// arrange
			var autoMocker = new AutoMocker();
			var distributedCache = new Mock<IDistributedCache>();
			var logger = new Mock<IBoltOnLogger<BoltOnCache>>();
			var sut = new BoltOnCache(distributedCache.Object, logger.Object);

			// act
			await sut.SetAsync("TestKey", "TestValue");

			// assert
			logger.Verify(l => l.Debug($"Setting value in cache... Key: TestKey"));
		}
	}
}
