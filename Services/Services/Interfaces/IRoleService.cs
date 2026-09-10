using Services.DTOs.Roles;
using Services.DTOs.Shared;

namespace Services.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<PagedResult<RoleDto>> GetPagedAsync(PagedRequest request);
        Task<RoleDto> GetByIdAsync(int id);
        Task<IEnumerable<PermissionOptionDto>> GetPermissionsAsync();
        Task<RoleDto> CreateAsync(RoleRequestDto request);
        Task<RoleDto> UpdateAsync(int id, RoleRequestDto request);
        Task DeleteAsync(int id);
    }
}
