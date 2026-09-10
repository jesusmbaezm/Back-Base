using AutoMapper;
using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.Roles;
using Services.DTOs.Shared;
using Services.Services.Interfaces;
using Services.Exceptions;

namespace Services.Services.Implementations
{
    public class RoleService : ServiceBase, IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IMapper _mapper;

        public RoleService(
            IRoleRepository roleRepository,
            IMapper mapper,
            IAuditLogService auditLogService,
            IUserContextService userContextService) : base(userContextService)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        public async Task<RoleDto> CreateAsync(RoleRequestDto request)
        {
            await ValidateRequestAsync(request);

            var permissionIds = request.PermissionIds ?? [];
            var permissions = await GetValidatedPermissionsAsync(permissionIds);
            var role = new Role
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                IsSystem = false,
                RolePermissions = permissions
                    .Select(permission => new RolePermission { PermissionId = permission.Id })
                    .ToList()
            };

            await _roleRepository.AddAsync(role);
            await _roleRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                tableName: "Roles",
                action: "CREATE",
                newValues: new
                {
                    role.Id,
                    role.Name,
                    role.Description,
                    PermissionIds = permissions.Select(p => p.Id).ToList()
                },
                primaryKey: role.Id.ToString());

            return await GetByIdAsync(role.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var role = await GetRoleWithPermissionsAsync(id);

            if (role.IsSystem)
            {
                throw new ValidationException("Los roles del sistema no se pueden eliminar.");
            }

            if (await _roleRepository.HasUsersAsync(id))
            {
                throw new ConflictException("El rol no se puede eliminar porque tiene usuarios asignados.");
            }

            var oldValues = new
            {
                role.Id,
                role.Name,
                role.Description,
                role.IsSystem,
                PermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList()
            };

            _roleRepository.Remove(role);
            await _roleRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                tableName: "Roles",
                action: "DELETE",
                oldValues: oldValues,
                primaryKey: id.ToString());
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<PagedResult<RoleDto>> GetPagedAsync(PagedRequest request)
        {
            var (items, totalCount) = await _roleRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.Search,
                request.SortBy,
                request.SortDescending,
                q => q.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission));

            var mappedItems = _mapper.Map<IEnumerable<RoleDto>>(items);
            return PagedResult<RoleDto>.Create(mappedItems, totalCount, request);
        }

        public async Task<RoleDto> GetByIdAsync(int id)
        {
            return _mapper.Map<RoleDto>(await GetRoleWithPermissionsAsync(id));
        }

        public async Task<IEnumerable<PermissionOptionDto>> GetPermissionsAsync()
        {
            var permissions = await _roleRepository.GetAllPermissionsAsync();
            return _mapper.Map<IEnumerable<PermissionOptionDto>>(permissions);
        }

        public async Task<RoleDto> UpdateAsync(int id, RoleRequestDto request)
        {
            await ValidateRequestAsync(request, id);

            var role = await GetRoleWithPermissionsAsync(id);
            if (role.IsSystem)
            {
                throw new ValidationException("Los roles del sistema no se pueden editar.");
            }

            var permissionIds = request.PermissionIds ?? [];
            var permissions = await GetValidatedPermissionsAsync(permissionIds);
            var oldValues = new
            {
                role.Name,
                role.Description,
                PermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).OrderBy(x => x).ToList()
            };

            role.Name = request.Name.Trim();
            role.Description = request.Description.Trim();
            role.RolePermissions.Clear();

            foreach (var permission in permissions)
            {
                role.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }

            await _roleRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                tableName: "Roles",
                action: "UPDATE",
                oldValues: oldValues,
                newValues: new
                {
                    role.Name,
                    role.Description,
                    PermissionIds = permissions.Select(p => p.Id).OrderBy(x => x).ToList()
                },
                primaryKey: role.Id.ToString(),
                affectedColumns: "Name, Description, Permissions");

            return await GetByIdAsync(role.Id);
        }

        private async Task<List<Permission>> GetValidatedPermissionsAsync(IEnumerable<int> permissionIds)
        {
            var ids = permissionIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return [];
            }

            var permissions = await _roleRepository.GetPermissionsByIdsAsync(ids);
            var foundIds = permissions.Select(p => p.Id).ToHashSet();
            var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();

            if (missingIds.Count > 0)
            {
                throw new ValidationException($"Permisos no encontrados: {string.Join(", ", missingIds)}.");
            }

            return permissions;
        }

        private async Task<Role> GetRoleWithPermissionsAsync(int id)
        {
            return await _roleRepository.GetByIdWithPermissionsAsync(id)
                ?? throw new NotFoundException("Rol no encontrado.");
        }

        private async Task ValidateRequestAsync(RoleRequestDto request, int? roleId = null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ValidationException("El nombre del rol es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                throw new ValidationException("La descripcion del rol es obligatoria.");
            }

            if (await _roleRepository.ExistsByNameAsync(request.Name, roleId))
            {
                throw new ConflictException("Ya existe un rol con el mismo nombre.");
            }
        }
    }
}
