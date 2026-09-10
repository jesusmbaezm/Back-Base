using Controllers.Services;
using Data.Repositories.Implementations;
using Data.Repositories.Interfaces;
using Services.Services.Implementations;
using Services.Services.Interfaces;

namespace Controllers.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IParameterRepository, ParameterRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IParameterService, ParameterService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<IUserContextService, HttpUserContextService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<INotificationService, EmailNotificationService>();
            services.AddScoped<IEmailSmtpClientFactory, MailKitSmtpClientFactory>();
            services.AddScoped<ISmtpSettingsProvider, ParameterSmtpSettingsProvider>();
            return services;
        }
    }
}
