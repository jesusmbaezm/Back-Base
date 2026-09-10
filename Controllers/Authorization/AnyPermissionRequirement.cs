using Microsoft.AspNetCore.Authorization;

namespace Controllers.Authorization
{
    public class AnyPermissionRequirement : IAuthorizationRequirement
    {
        public string[] Permissions { get; }

        public AnyPermissionRequirement(string[] permissions)
        {
            Permissions = permissions;
        }
    }
}
