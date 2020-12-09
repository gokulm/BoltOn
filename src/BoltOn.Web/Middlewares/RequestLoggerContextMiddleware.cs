using System.Threading.Tasks;
using BoltOn.Logging;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BoltOn.Web.Middlewares
{
	public class RequestLoggerContextMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggerContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext,
            IBoltOnLogger<RequestLoggerContextMiddleware> logger,
            LoggerContext loggerContext,
            ICorrelationContextAccessor correlationContextAccessor)
        {
            loggerContext.SetByKey("ClientIp", httpContext.Connection.RemoteIpAddress.ToString());
            loggerContext.SetByKey("RequestUrl", httpContext.Request.Path.Value);
            loggerContext.SetByKey("CorrelationId", correlationContextAccessor?.CorrelationContext?.CorrelationId);

            logger.Debug($"Executing {nameof(RequestLoggerContextMiddleware)} ...");
            await _next(httpContext);
            logger.Debug($"Executed {nameof(RequestLoggerContextMiddleware)}");
        }
    }
}
