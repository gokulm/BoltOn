using System.Collections.Generic;
using System.Threading.Tasks;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
            ILogger<RequestLoggerContextMiddleware> logger,
            ICorrelationContextAccessor correlationContextAccessor,
            IConfiguration configuration)
        {
            using (logger.BeginScope(new Dictionary<string, object> { { "ClientIp", httpContext.Connection.RemoteIpAddress.ToString() } }))
            using (logger.BeginScope(new Dictionary<string, object> { { "RequestUrl", httpContext.Request.Path.Value } }))
            using (logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationContextAccessor?.CorrelationContext?.CorrelationId } }))
            using (logger.BeginScope(new Dictionary<string, object> { { "Module", configuration.GetValue<string>("Module") } }))
            {
                logger.LogDebug($"Executing {nameof(RequestLoggerContextMiddleware)} ...");
                await _next(httpContext);
                logger.LogDebug($"Executed {nameof(RequestLoggerContextMiddleware)}");
            }
        }
    }
}
