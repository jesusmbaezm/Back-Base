using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Controllers.Authorization;

namespace Controllers.Authorization
{
    /// <summary>
    /// Handles policies whose name starts with "any:" — e.g. "any:quotations.create|remissions.create".
    /// Falls back to the default provider for all other policy names.
    /// </summary>
    public class AnyPermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public const string PolicyPrefix = "any:";

        private readonly DefaultAuthorizationPolicyProvider _fallback;

        public AnyPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallback = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            _fallback.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
            _fallback.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var permissions = policyName[PolicyPrefix.Length..].Split('|');
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new AnyPermissionRequirement(permissions))
                    .Build();
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return _fallback.GetPolicyAsync(policyName);
        }
    }
}
