using AutoMapper;
using Data.Entities;
using Data.Repositories.Interfaces;
using Services.Constants;
using Services.DTOs.Parameters;
using Services.DTOs.Shared;
using Services.Services.Interfaces;
using Services.Exceptions;

namespace Services.Services.Implementations;

public class ParameterService : IParameterService
{
    private readonly IParameterRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditService;

    public ParameterService(IParameterRepository repository, IMapper mapper, IAuditLogService auditService)
    {
        _repository = repository;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<IEnumerable<ParameterDto>> GetAllAsync()
    {
        var parameters = await _repository.GetAllAsync();
        return parameters.Select(MapToDto);
    }

    public async Task<ParameterDto?> GetByIdAsync(int id)
    {
        var parameter = await _repository.GetByIdAsync(id);
        if (parameter is null) return null;
        return MapToDto(parameter);
    }

    public async Task<ParameterDto> CreateAsync(CreateParameterDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ValidationException("La descripcion del parametro es obligatoria.");
        }

        var parameter = _mapper.Map<Parameter>(dto);
        parameter.Name = dto.Name.Trim();
        parameter.Description = dto.Description.Trim();
        parameter.Value = dto.Value;
        parameter.ModificationDate = DateTimeOffset.UtcNow;
        parameter.ModifiedByUserId = userId;

        var created = await _repository.CreateAsync(parameter);

        await _auditService.LogAsync(
            tableName: "Parameters",
            action: "CREATE",
            newValues: BuildAuditValues(created),
            primaryKey: created.Id.ToString()
        );

        return MapToDto(created);
    }

    public async Task<ParameterDto?> UpdateAsync(int id, UpdateParameterDto dto, int userId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        var oldValues = BuildAuditValues(existing);

        existing.Name = dto.Name.Trim();
        existing.Value = dto.Value;
        existing.ModificationDate = DateTimeOffset.UtcNow;
        existing.ModifiedByUserId = userId;

        var updated = await _repository.UpdateAsync(existing);

        await _auditService.LogAsync(
            tableName: "Parameters",
            action: "UPDATE",
            oldValues: oldValues,
            newValues: BuildAuditValues(updated),
            primaryKey: updated.Id.ToString(),
            affectedColumns: "Name, Value"
        );

        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return false;

        var deleted = await _repository.DeleteAsync(id);

        if (deleted)
        {
            await _auditService.LogAsync(
                tableName: "Parameters",
                action: "DELETE",
                oldValues: BuildAuditValues(existing),
                primaryKey: id.ToString()
            );
        }

        return deleted;
    }

    public async Task<PagedResult<ParameterDto>> GetPagedAsync(PagedRequest request)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.SortBy,
            request.SortDescending);

        var mappedItems = _mapper.Map<IEnumerable<ParameterDto>>(items);
        return PagedResult<ParameterDto>.Create(mappedItems.Select(ApplyMask), totalCount, request);
    }

    private static object BuildAuditValues(Parameter parameter)
    {
        return new
        {
            parameter.Id,
            parameter.Name,
            parameter.Description,
            Value = MaskValue(parameter.Name, parameter.Value)
        };
    }

    private static ParameterDto ApplyMask(ParameterDto parameter)
    {
        parameter.Value = MaskValue(parameter.Name, parameter.Value);
        return parameter;
    }

    private static ParameterDto MapToDto(Parameter parameter)
    {
        return new ParameterDto
        {
            Id = parameter.Id,
            Name = parameter.Name,
            Description = parameter.Description,
            Value = MaskValue(parameter.Name, parameter.Value),
            ModificationDate = parameter.ModificationDate,
            ModifiedByUserId = parameter.ModifiedByUserId
        };
    }

    private static string MaskValue(string parameterName, string? value)
    {
        if (!ParameterNames.Sensitive.Contains(parameterName))
        {
            return value ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(value) ? string.Empty : "********";
    }
}
