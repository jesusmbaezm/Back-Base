namespace Services.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string tableName, string action, object? oldValues = null, object? newValues = null, string? primaryKey = null, string? affectedColumns = null);
    }
}
