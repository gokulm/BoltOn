using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BoltOn.Web.Authorization
{
	public class AppPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _defaultAuthorizationPolicyProvider;

        public AppPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _defaultAuthorizationPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _defaultAuthorizationPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _defaultAuthorizationPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith("Scope", StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new ScopeRequirement(policyName));
                return Task.FromResult(policy.Build());
            }

            if (policyName.StartsWith("Permission", StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirement(policyName));
                return Task.FromResult(policy.Build());
            }

            return _defaultAuthorizationPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}

