using Data.Contexts;
using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsActiveByIdAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id && !u.IsDeleted && u.IsActive);
        }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        {
            var normalizedEmail = email.Trim();

            return await _context.Users.AnyAsync(u =>
                !u.IsDeleted &&
                u.Email == normalizedEmail &&
                (!excludeId.HasValue || u.Id != excludeId.Value));
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, int? roleId = null, bool? isActive = null, Func<IQueryable<User>, IQueryable<User>>? queryShaper = null)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted);

            if (queryShaper != null)
            {
                query = queryShaper(query);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search));
            }

            if (roleId.HasValue)
            {
                query = query.Where(u => u.UserRoles.Any(userRole => userRole.RoleId == roleId.Value));
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            query = sortBy?.ToLowerInvariant() switch
            {
                "name" => sortDescending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name),
                "email" => sortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "isactive" => sortDescending ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
                _ => query.OrderBy(u => u.Name)
            };

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.IsActive);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<User?> GetTrackedByIdIncludingDeletedAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByIdWithRolesAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<User?> GetByIdWithRolesAndPermissionsAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted && u.IsActive);
        }

        public async Task<List<Role>> GetRolesByIdsAsync(IEnumerable<int> ids)
        {
            var roleIds = ids.Distinct().ToList();

            return await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
