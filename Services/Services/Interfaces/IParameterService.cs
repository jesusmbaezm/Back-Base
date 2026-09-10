using Services.DTOs.Parameters;
using Services.DTOs.Shared;

namespace Services.Services.Interfaces
{
    public interface IParameterService
    {
        Task<IEnumerable<ParameterDto>> GetAllAsync();
        Task<ParameterDto?> GetByIdAsync(int id);
        Task<ParameterDto> CreateAsync(CreateParameterDto dto, int userId);
        Task<ParameterDto?> UpdateAsync(int id, UpdateParameterDto dto, int userId);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<ParameterDto>> GetPagedAsync(PagedRequest request);
    }
}
