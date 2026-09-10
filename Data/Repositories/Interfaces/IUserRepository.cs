using Data.Entities;

namespace Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsActiveByIdAsync(int id);
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
        Task<List<User>> GetAllAsync();
        Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, int? roleId = null, bool? isActive = null, Func<IQueryable<User>, IQueryable<User>>? queryShaper = null);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetTrackedByIdIncludingDeletedAsync(int id);
        Task<User?> GetByIdWithRolesAsync(int id);
        Task<User?> GetByIdWithRolesAndPermissionsAsync(int id);
        Task<List<Role>> GetRolesByIdsAsync(IEnumerable<int> ids);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
