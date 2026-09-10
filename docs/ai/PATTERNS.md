# Patterns

## Purpose
Shared implementation patterns already established in the repository.

## Controller Pattern
- Use `[ApiController]`, stable plural routes, and `BaseController` response helpers.
- Keep controllers thin and delegate to services.
- Protect endpoints with `HasPermission(...)` when applicable.
- `DELETE` endpoints should return a wrapped success response through `SuccessEmptyResponse(...)` instead of a bare `204 No Content`.

## Paging Pattern
- Use `PagedRequest` for query input.
- Use `PagedResult<T>.Create(...)` for service responses.
- Repositories implement `GetPagedAsync(page, pageSize, search, sortBy, sortDescending, queryShaper)` when the module needs paged listing.

## Mapping Pattern
- Entity-to-response projection belongs in AutoMapper profiles under `SIGJ.Services/Mappings/`.
- Services should return DTOs mapped from repository-loaded entities.
- User-facing enum translations should be resolved through AutoMapper projection or centralized helpers, not ad-hoc in controllers.
- Prefer helper methods such as `ToSpanishLabel(...)` for enum translation reuse across DTOs, exports, and user-visible messages.
- When a public response already exposes a technical enum value, prefer `TechnicalField + LabelField` over replacing the technical field.

## Audit Pattern
- Mutating services call `IAuditLogService.LogAsync(...)`.
- Include business-relevant old and new values when possible.

## Logical Delete Pattern
- Services call shared audit helpers to mark delete and modification timestamps.
- Repositories filter out deleted rows in standard queries.

