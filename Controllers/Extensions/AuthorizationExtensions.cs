using Controllers.Authorization;
using Microsoft.AspNetCore.Authorization;
using Services.Constants;
using Data.Entities;

namespace Controllers.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton<IAuthorizationHandler, AnyPermissionHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, AnyPermissionPolicyProvider>();

            services.AddAuthorization(options =>
            {
                options.AddPermissionPolicies(
                    Permissions.Parameter.Read,
                    Permissions.Parameter.Update,
                    Permissions.User.Read,
                    Permissions.User.Create,
                    Permissions.User.Update,
                    Permissions.User.Delete,
                    Permissions.User.ResetPassword,
                    Permissions.Role.Read,
                    Permissions.Role.Create,
                    Permissions.Role.Update,
                    Permissions.Role.Delete

                );
            });

            return services;
        }

        private static void AddPermissionPolicies(this AuthorizationOptions options, params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                options.AddPolicy(permission, policy =>
                    policy.AddRequirements(new PermissionRequirement(permission)));
            }
        }
    }
}
