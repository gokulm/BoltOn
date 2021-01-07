using System;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Web.Middlewares;
using CorrelationId;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;
using Xunit;

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
            var logger = new Mock<IAppLogger<RequestLoggerContextMiddleware>>();
			var httpContext = new Mock<HttpContext>();
            var connectionInfo = new Mock<ConnectionInfo>();
            httpContext.Setup(s => s.Connection).Returns(connectionInfo.Object);
            var request = Mock.Of<HttpRequest>();
            request.Path = new PathString("/test");
            httpContext.Setup(s => s.Request).Returns(request);
            connectionInfo.Setup(s => s.RemoteIpAddress).Returns(new System.Net.IPAddress(34));
            var loggerContext = new Mock<LoggerContext>();
            var correlationContextAccessor = new Mock<ICorrelationContextAccessor>();
            var correlationId = Guid.NewGuid().ToString();
            var correlationContext = new CorrelationContext(correlationId, "test header");
            correlationContextAccessor.Setup(s => s.CorrelationContext).Returns(correlationContext);

            // act
            await sut.Invoke(httpContext.Object, logger.Object, loggerContext.Object, correlationContextAccessor.Object);

            // assert
            logger.Verify(v => v.Debug("Model is invalid"), Times.Never);
            logger.Verify(v => v.Debug($"Executing {nameof(RequestLoggerContextMiddleware)} ..."));
            logger.Verify(v => v.Debug($"Executed {nameof(RequestLoggerContextMiddleware)}"));
            loggerContext.Verify(v => v.SetByKey("ClientIp", "34.0.0.0"));
            loggerContext.Verify(v => v.SetByKey("RequestUrl", "/test"));
            loggerContext.Verify(v => v.SetByKey("CorrelationId", correlationId.ToString()));
        }
    }
}
