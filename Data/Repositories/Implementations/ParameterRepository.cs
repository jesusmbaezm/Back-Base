using Data.Contexts;
using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories.Implementations
{
    public class ParameterRepository : IParameterRepository
    {
        private readonly AppDbContext _context;

        public ParameterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Parameter>> GetAllAsync()
        {
            return await _context.Parameters.AsNoTracking().ToListAsync();
        }

        public async Task<Parameter?> GetByNameAsync(string name)
        {
            return await _context.Parameters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<Parameter?> GetByIdAsync(int id)
        {
            return await _context.Parameters.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Parameter> CreateAsync(Parameter parameter)
        {
            _context.Parameters.Add(parameter);
            await _context.SaveChangesAsync();
            return parameter;
        }

        public async Task<Parameter> UpdateAsync(Parameter parameter)
        {
            var existing = await _context.Parameters.FindAsync(parameter.Id);
            if (existing is null) throw new Exception("Parámetro no encontrado");

            _context.Entry(existing).CurrentValues.SetValues(parameter);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var parameter = await _context.Parameters.FindAsync(id);
            if (parameter is null) return false;

            _context.Parameters.Remove(parameter);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Parameter> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search, string? sortBy, bool sortDescending)
        {
            var query = _context.Parameters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) ||
                                         p.Value.Contains(search) ||
                                         (p.Description != null && p.Description.Contains(search)));

            var totalCount = await query.CountAsync();

            query = sortBy?.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "description" => sortDescending ? query.OrderByDescending(p => p.Description) : query.OrderBy(p => p.Description),
                "value" => sortDescending ? query.OrderByDescending(p => p.Value) : query.OrderBy(p => p.Value),
                _ => query.OrderBy(p => p.Id)
            };

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public IQueryable<Parameter> Query(bool asNoTracking = true)
        {
            var q = _context.Set<Parameter>().AsQueryable();
            return asNoTracking ? q.AsNoTracking() : q;
        }
    }
}
