using Data.Entities;

namespace Data.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllAsync();
        Task<(IEnumerable<Role> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, Func<IQueryable<Role>, IQueryable<Role>>? queryShaper = null);
        Task<Role?> GetByIdAsync(int id);
        Task<Role?> GetByIdWithPermissionsAsync(int id);
        Task<Role?> GetByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task<List<Permission>> GetPermissionsByIdsAsync(IEnumerable<int> ids);
        Task<List<Permission>> GetAllPermissionsAsync();
        Task AddAsync(Role role);
        void Remove(Role role);
        Task<bool> HasUsersAsync(int roleId);
        Task SaveChangesAsync();
    }
}
