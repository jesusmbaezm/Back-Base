namespace Services.Services.Interfaces
{
    public interface IUserContextService
    {
        int GetCurrentUserId();
        IReadOnlyCollection<int> GetAssignedBranchIds();
        bool HasPermission(string permission);
        string GetCurrentUserName();
    }
}
