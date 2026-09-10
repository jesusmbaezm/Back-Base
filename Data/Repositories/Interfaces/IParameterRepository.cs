using Data.Entities;

namespace Data.Repositories.Interfaces
{
    public interface IParameterRepository
    {
        Task<IEnumerable<Parameter>> GetAllAsync();
        Task<Parameter> CreateAsync(Parameter parameter);
        Task<Parameter> UpdateAsync(Parameter parameter);
        Task<bool> DeleteAsync(int id);
        Task<Parameter?> GetByIdAsync(int id);
        Task<Parameter?> GetByNameAsync(string name);
        Task<(IEnumerable<Parameter> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false);
        IQueryable<Parameter> Query(bool asNoTracking = true);
    }
}
