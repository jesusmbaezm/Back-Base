using Microsoft.AspNetCore.Authorization;

namespace Controllers.Authorization
{
    public class AnyPermissionHandler : AuthorizationHandler<AnyPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AnyPermissionRequirement requirement)
        {
            var hasAny = requirement.Permissions.Any(p =>
                context.User.Claims.Any(c => c.Type == "permission" && c.Value == p));

            if (hasAny)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
