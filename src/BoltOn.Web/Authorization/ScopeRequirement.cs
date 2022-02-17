using Microsoft.AspNetCore.Authorization;

namespace BoltOn.Web.Authorization
{
	public class ScopeRequirement : IAuthorizationRequirement
    {
        public string Scope { get; private set; }

        public ScopeRequirement(string scope)
        {
            Scope = scope;
        }
    }
}

