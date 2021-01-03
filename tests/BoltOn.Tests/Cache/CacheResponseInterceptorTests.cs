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
        private const string CacheKey = "TestKey";
        private const string CacheValue = "TestValue";

        [Fact]
        public async Task RunAsync_RequestThatDoesNotImplementICacheResponse_CallsNextDelegate()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var logger = autoMocker.GetMock<IAppLogger<CacheResponseInterceptor>>();

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
            var logger = autoMocker.GetMock<IAppLogger<CacheResponseInterceptor>>();

            var boltOnCache = autoMocker.GetMock<IBoltOnCache>();
            boltOnCache.Setup(b => b.GetAsync<string>(CacheKey, It.IsAny<CancellationToken>(),
                null, It.IsAny<TimeSpan?>())).Returns(Task.FromResult(CacheValue));
            Func<TestRequest, CancellationToken, Task<string>> nextDelegate =
                (r, c) => new Mock<IHandler<TestRequest, string>>().Object.HandleAsync(r, c);
            var sut = autoMocker.CreateInstance<CacheResponseInterceptor>();

            // act
            await sut.RunAsync(new TestRequest(), default, nextDelegate);

            // assert
            logger.Verify(l => l.Debug("CacheResponseInterceptor started"));
            logger.Verify(l => l.Debug($"Retrieving response from cache. Key: {CacheKey}"));
            logger.Verify(l => l.Debug("Returning response from cache"));
            boltOnCache.Verify(b => b.GetAsync<string>(CacheKey, It.IsAny<CancellationToken>(),
                null, It.IsAny<TimeSpan?>()));
        }

        [Fact]
        public async Task RunAsync_ResponseNotInCache_SavesResponseInCache()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var logger = autoMocker.GetMock<IAppLogger<CacheResponseInterceptor>>();

            var boltOnCache = autoMocker.GetMock<IBoltOnCache>();
            boltOnCache.Setup(b => b.GetAsync<string>(CacheKey, It.IsAny<CancellationToken>(),
                null, It.IsAny<TimeSpan?>())).Returns(Task.FromResult((string)null));
            var handler = new Mock<IHandler<TestRequest, string>>();
            var testRequest = new TestRequest();
            handler.Setup(s => s.HandleAsync(testRequest, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CacheValue));
            Func<TestRequest, CancellationToken, Task<string>> nextDelegate =
                (r, c) => handler.Object.HandleAsync(r, c);
            var sut = autoMocker.CreateInstance<CacheResponseInterceptor>();

            // act
            await sut.RunAsync(testRequest, default, nextDelegate);

            // assert
            logger.Verify(l => l.Debug("CacheResponseInterceptor started"));
            logger.Verify(l => l.Debug($"Retrieving response from cache. Key: {CacheKey}"));
            logger.Verify(l => l.Debug("CacheResponseInterceptor ended"));
            handler.Verify(s => s.HandleAsync(testRequest, It.IsAny<CancellationToken>()));
            boltOnCache.Verify(b => b.GetAsync<string>(CacheKey, It.IsAny<CancellationToken>(),
                null, It.IsAny<TimeSpan?>()));
            logger.Verify(l => l.Debug($"Saving response in cache. Key: {CacheKey}"));
            boltOnCache.Verify(b => b.SetAsync(CacheKey, CacheValue, It.IsAny<CancellationToken>(),
                 It.IsAny<TimeSpan?>()));
        }

        [Fact]
        public async Task RunAsync_ClearCacheResponseRequest_RemovesResponseFromCache()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var logger = autoMocker.GetMock<IAppLogger<CacheResponseInterceptor>>();

            var boltOnCache = autoMocker.GetMock<IBoltOnCache>();
            var handler = new Mock<IHandler<TestClearCacheRequest, string>>();
            var testRequest = new TestClearCacheRequest();
            handler.Setup(s => s.HandleAsync(testRequest, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CacheValue));
            Func<TestClearCacheRequest, CancellationToken, Task<string>> nextDelegate =
                (r, c) => handler.Object.HandleAsync(r, c);
            var sut = autoMocker.CreateInstance<CacheResponseInterceptor>();

            // act
            await sut.RunAsync(testRequest, default, nextDelegate);

            // assert
            logger.Verify(l => l.Debug("CacheResponseInterceptor started"));
            logger.Verify(l => l.Debug("CacheResponseInterceptor ended"));
            handler.Verify(s => s.HandleAsync(testRequest, It.IsAny<CancellationToken>()));
            logger.Verify(l => l.Debug($"Retrieving response from cache. Key: {CacheKey}"), Times.Never);
            logger.Verify(l => l.Debug($"Removing response from cache. Key: {CacheKey}"));
            logger.Verify(l => l.Debug($"Saving response in cache. Key: {CacheKey}"), Times.Never);
            boltOnCache.Verify(b => b.RemoveAsync(CacheKey, It.IsAny<CancellationToken>()));
        }
    }
}
