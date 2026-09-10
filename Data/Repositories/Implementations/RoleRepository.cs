using Data.Contexts;
using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories.Implementations
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var normalizedName = name.Trim();

            return await _context.Roles
                .AnyAsync(r => r.Name == normalizedName && (!excludeId.HasValue || r.Id != excludeId.Value));
        }

        public async Task<List<Role>> GetAllAsync()
        {
            return await _context.Roles
                .AsNoTracking()
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Role> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, Func<IQueryable<Role>, IQueryable<Role>>? queryShaper = null)
        {
            var query = _context.Roles.AsNoTracking();

            if (queryShaper != null)
            {
                query = queryShaper(query);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.Name.Contains(search) ||
                    r.Description.Contains(search));
            }

            var totalCount = await query.CountAsync();

            query = sortBy?.ToLowerInvariant() switch
            {
                "name" => sortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                "description" => sortDescending ? query.OrderByDescending(r => r.Description) : query.OrderBy(r => r.Description),
                _ => query.OrderBy(r => r.Name)
            };

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .AsNoTracking()
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role?> GetByIdWithPermissionsAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            var normalizedName = name.Trim();

            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == normalizedName);
        }

        public async Task<List<Permission>> GetPermissionsByIdsAsync(IEnumerable<int> ids)
        {
            var permissionIds = ids.Distinct().ToList();

            return await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<bool> HasUsersAsync(int roleId)
        {
            return await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId);
        }

        public void Remove(Role role)
        {
            _context.Roles.Remove(role);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
