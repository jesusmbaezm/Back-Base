using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Services.Services.Interfaces;
using Data.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace Services.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string tableName, string action, object? oldValues = null, object? newValues = null, string? primaryKey = null, string? affectedColumns = null)
        {
            var (userId, userEmail, ipAddress) = GetRequestContext();

            var log = new AuditLog
            {
                TableName = tableName,
                Action = action,
                OldValues = Serialize(oldValues),
                NewValues = Serialize(newValues),
                AffectedColumns = affectedColumns,
                PrimaryKey = primaryKey,
                UserId = userId,
                UserEmail = userEmail,
                IpAddress = ipAddress,
                Date = DateTimeOffset.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private (int? userId, string? userEmail, string? ipAddress) GetRequestContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var rawId = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? httpContext?.User?.FindFirst("sub")?.Value;
            var userEmail = httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                         ?? httpContext?.User?.FindFirst("email")?.Value;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();

            return (int.TryParse(rawId, out var id) ? id : null, userEmail, ipAddress);
        }

        private static string? Serialize(object? value)
            => value is not null ? JsonSerializer.Serialize(value) : null;
    }
}
