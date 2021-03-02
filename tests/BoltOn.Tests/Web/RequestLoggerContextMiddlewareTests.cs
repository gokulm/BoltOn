using System;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Web.Middlewares;
using CorrelationId;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Xunit;
using BoltOn.Tests.Common;
using Microsoft.Extensions.Configuration;

namespace BoltOn.Tests.Web
{
    public class RequestLoggerContextMiddlewareTests
    {
        [Fact]
        public async Task Invoke_ValidInput_AddsAllTheAttributesToLoggerContext()
        {
            // arrange
            var autoMocker = new AutoMocker();
            var sut = autoMocker.CreateInstance<RequestLoggerContextMiddleware>();
            var logger = new Mock<ILogger<RequestLoggerContextMiddleware>>();
            var httpContext = new Mock<HttpContext>();
            var connectionInfo = new Mock<ConnectionInfo>();
            httpContext.Setup(s => s.Connection).Returns(connectionInfo.Object);
            var request = Mock.Of<HttpRequest>();
            request.Path = new PathString("/test");
            httpContext.Setup(s => s.Request).Returns(request);
            connectionInfo.Setup(s => s.RemoteIpAddress).Returns(new System.Net.IPAddress(34));
            var correlationContextAccessor = new Mock<ICorrelationContextAccessor>();
            var correlationId = Guid.NewGuid().ToString();
            var correlationContext = new CorrelationContext(correlationId, "test header");
            correlationContextAccessor.Setup(s => s.CorrelationContext).Returns(correlationContext);
            var configuration = autoMocker.GetMock<IConfiguration>();
            configuration.Setup(s => s.GetSection(It.IsAny<string>())).Returns(Mock.Of<IConfigurationSection>());

            // act
            await sut.Invoke(httpContext.Object, logger.Object, correlationContextAccessor.Object, configuration.Object);

            // assert
            logger.VerifyDebug($"Executing {nameof(RequestLoggerContextMiddleware)} ...");
            logger.VerifyDebug($"Executed {nameof(RequestLoggerContextMiddleware)}");
        }
    }
}
