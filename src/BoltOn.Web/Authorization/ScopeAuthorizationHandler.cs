using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BoltOn.Web.Authorization
{
	public class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
        {
            if (context.User == null)
                return Task.CompletedTask;

            var user = context.User;
            var tempScopeClaim = user.Claims.FirstOrDefault(f => f.Type == "scope");
            if (tempScopeClaim != null)
            {
                var scopeClaims = tempScopeClaim.Value.Split(' ');
                if (scopeClaims.Any(a => a == requirement.Scope))
                    context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

