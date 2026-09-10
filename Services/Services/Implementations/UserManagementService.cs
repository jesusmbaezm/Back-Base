using AutoMapper;
using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.DTOs.Shared;
using Services.DTOs.Users;
using Services.Services.Interfaces;
using Services.Exceptions;

namespace Services.Services.Implementations
{
    public class UserManagementService : ServiceBase, IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IMapper _mapper;

        public UserManagementService(
            IUserRepository userRepository,
            IMapper mapper,
            IAuditLogService auditLogService,
            IUserContextService userContextService) : base(userContextService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        public async Task<UserDto> CreateAsync(CreateUserDto request)
        {
            await ValidateCreateAsync(request);

            var roleIds = request.RoleIds ?? [];
            var roles = await GetValidatedRolesAsync(roleIds);
            var user = new User
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true,
                UserRoles = roles.Select(role => new UserRole { RoleId = role.Id }).ToList()
            };

            SetCreationAudit(user);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                tableName: "Users",
                action: "CREATE",
                newValues: new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.IsActive,
                    RoleIds = roles.Select(r => r.Id).OrderBy(x => x).ToList(),
                },
                primaryKey: user.Id.ToString());

            return await GetByIdAsync(user.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _userRepository.GetTrackedByIdIncludingDeletedAsync(id)
                ?? throw new NotFoundException("Usuario no encontrado.");

            if (user.IsDeleted)
            {
                return;
            }

            if (user.UserRoles.Any(userRole => userRole.Role != null && userRole.Role.IsSystem))
            {
                throw new ValidationException("No se pueden eliminar usuarios con roles de sistema.");
            }

            var oldValues = new
            {
                user.Name,
                user.Email,
                user.IsActive,
                user.IsDeleted
            };

            SetDeleted(user);
            user.IsActive = false;
            await _userRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                tableName: "Users",
                action: "DELETE",
                oldValues: oldValues,
                newValues: new { user.IsActive, user.IsDeleted },
                primaryKey: user.Id.ToString(),
                affectedColumns: "IsActive, IsDeleted");
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<PagedResult<UserDto>> GetPagedAsync(UserPagedRequestDto request)
        {
            var (items, totalCount) = await _userRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.Search,
                request.SortBy,
                request.SortDescending,
                request.RoleId,
                request.IsActive,
                q => q
                    .Include(u => u.UserRoles));

            var mappedItems = _mapper.Map<IEnumerable<UserDto>>(items);
            return PagedResult<UserDto>.Create(mappedItems, totalCount, request);
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            return _mapper.Map<UserDto>(await GetUserWithRolesAsync(id));
        }

        public async Task ResetPasswordAsync(int id, ResetUserPasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new ValidationException("La nueva contrasena es obligatoria.");
            }

            var user = await GetUserAsync(id);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            SetModificationAudit(user);
            await _userRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                tableName: "Users",
                action: "RESET_PASSWORD",
                newValues: new { user.Id, PasswordResetByUserId = GetCurrentUserId() },
                primaryKey: user.Id.ToString(),
                affectedColumns: "PasswordHash");
        }

        public async Task<UserDto> UpdateAsync(int id, UpdateUserDto request)
        {
            await ValidateUpdateAsync(id, request);

            var user = await GetUserWithRolesAsync(id);
            var roleIds = request.RoleIds ?? [];
            var roles = await GetValidatedRolesAsync(roleIds);
            var oldValues = new
            {
                user.Name,
                user.Email,
                user.IsActive,
                RoleIds = user.UserRoles.Select(ur => ur.RoleId).OrderBy(x => x).ToList(),
            };

            user.Name = request.Name.Trim();
            user.Email = request.Email.Trim();
            user.IsActive = request.IsActive;
            user.UserRoles.Clear();

            foreach (var role in roles)
            {
                user.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }

            SetModificationAudit(user);

            await _userRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                tableName: "Users",
                action: "UPDATE",
                oldValues: oldValues,
                newValues: new
                {
                    user.Name,
                    user.Email,
                    user.IsActive,
                    RoleIds = roles.Select(r => r.Id).OrderBy(x => x).ToList(),
                },
                primaryKey: user.Id.ToString(),
                affectedColumns: "Name, Email, IsActive, Roles, Branches");

            return await GetByIdAsync(user.Id);
        }

        private async Task<List<Role>> GetValidatedRolesAsync(IEnumerable<int> roleIds)
        {
            var ids = roleIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return [];
            }

            var roles = await _userRepository.GetRolesByIdsAsync(ids);
            var foundIds = roles.Select(r => r.Id).ToHashSet();
            var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();

            if (missingIds.Count > 0)
            {
                throw new ValidationException($"Roles no encontrados: {string.Join(", ", missingIds)}.");
            }

            return roles;
        }

        private async Task<User> GetUserAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Usuario no encontrado.");
        }

        private async Task<User> GetUserWithRolesAsync(int id)
        {
            return await _userRepository.GetByIdWithRolesAsync(id)
                ?? throw new NotFoundException("Usuario no encontrado.");
        }

        private async Task ValidateCreateAsync(CreateUserDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ValidationException("El nombre del usuario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ValidationException("El correo del usuario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException("La contrasena del usuario es obligatoria.");
            }

            if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                throw new ConflictException("Ya existe un usuario con el mismo correo.");
            }
        }

        private async Task ValidateUpdateAsync(int id, UpdateUserDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ValidationException("El nombre del usuario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ValidationException("El correo del usuario es obligatorio.");
            }

            if (await _userRepository.ExistsByEmailAsync(request.Email, id))
            {
                throw new ConflictException("Ya existe un usuario con el mismo correo.");
            }
        }
    }
}
