using System.Threading.Tasks;
using BoltOn.Web.Authorization;
using Microsoft.AspNetCore.Http;

namespace BoltOn.Web.Middlewares
{
	public class UserClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        public UserClaimsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IClaimsService claimsService)
        {
            var user = httpContext.User;
            if (user != null && user.Identity.IsAuthenticated)
            {
                claimsService.LoadClaims(user);
            }
            await _next(httpContext);
        }
    }
}

