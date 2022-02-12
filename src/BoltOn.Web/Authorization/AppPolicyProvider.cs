using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace BoltOn.Web.Authorization
{
	public class AppPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _defaultAuthorizationPolicyProvider;
		private readonly IConfiguration _configuration;

		public AppPolicyProvider(IOptions<AuthorizationOptions> options,
            IConfiguration configuration)
        {
            _defaultAuthorizationPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
			_configuration = configuration;
		}

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _defaultAuthorizationPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _defaultAuthorizationPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            var scopePolicyPrefix = _configuration.GetValue<string>("ScopePolicyPrefix");
            var permissionPolicyPrefix = _configuration.GetValue<string>("PermissionPolicyPrefix");

            if (string.IsNullOrWhiteSpace(scopePolicyPrefix))
                scopePolicyPrefix = "Scope";
            if (string.IsNullOrWhiteSpace(scopePolicyPrefix))
                permissionPolicyPrefix = "Permission";

            if (policyName.StartsWith(scopePolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new ScopeRequirement(policyName));
                return Task.FromResult(policy.Build());
            }

            if (policyName.StartsWith(permissionPolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirement(policyName));
                return Task.FromResult(policy.Build());
            }

            return _defaultAuthorizationPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}

