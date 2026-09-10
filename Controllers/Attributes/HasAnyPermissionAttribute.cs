using Controllers.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Controllers.Attributes
{
    /// <summary>
    /// Authorizes a request if the user has ANY of the specified permissions.
    /// Use this for lookup/supporting endpoints consumed by multiple modules.
    /// </summary>
    public class HasAnyPermissionAttribute : AuthorizeAttribute
    {
        public HasAnyPermissionAttribute(params string[] permissions)
            : base(AnyPermissionPolicyProvider.PolicyPrefix + string.Join("|", permissions))
        {
        }
    }
}
