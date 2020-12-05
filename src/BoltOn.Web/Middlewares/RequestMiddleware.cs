using System.Threading.Tasks;
using BoltOn.Logging;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BoltOn.Web.Middlewares
{
	public class RequestMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext,
            IBoltOnLogger<RequestMiddleware> logger,
            LoggerContext loggerContext,
            ICorrelationContextAccessor correlationContextAccessor)
        {
            loggerContext.SetByKey("ClientIp", httpContext.Connection.RemoteIpAddress.ToString());
            loggerContext.SetByKey("RequestUrl", httpContext.Request.Path.Value);
            loggerContext.SetByKey("CorrelationId", correlationContextAccessor?.CorrelationContext?.CorrelationId);

            logger.Debug($"Executing {nameof(RequestMiddleware)} ...");
            await _next(httpContext);
            logger.Debug($"Executed {nameof(RequestMiddleware)}");
        }
    }
}
