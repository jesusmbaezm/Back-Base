using Services.DTOs.Shared;
using Services.DTOs.Users;

namespace Services.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<PagedResult<UserDto>> GetPagedAsync(UserPagedRequestDto request);
        Task<UserDto> GetByIdAsync(int id);
        Task<UserDto> CreateAsync(CreateUserDto request);
        Task<UserDto> UpdateAsync(int id, UpdateUserDto request);
        Task DeleteAsync(int id);
        Task ResetPasswordAsync(int id, ResetUserPasswordDto request);
    }
}
