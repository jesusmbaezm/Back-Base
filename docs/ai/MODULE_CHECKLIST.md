# Module Checklist

## Purpose
Required checklist for any new module or major module expansion.

## Naming And Structure
- Use English for entities, DTOs, controllers, repositories, permissions, SQL-facing names, and code identifiers.
- Follow the vertical slice pattern: controller, service, DTOs, repository, entity, EF configuration, mapping, permissions, migration.
- Keep dependency flow `Controllers -> Services -> Data`.

## Persistence
- If the module stores user-managed data, include logical delete and audit fields.
- Add EF configuration and register the entity in `AppDbContext`.
- Create a migration for schema changes.
- Apply logical delete filters in list and get-by-id queries.

## API Shape
- Add CRUD endpoints when the module follows the standard resource pattern.
- Add `paged` endpoints for list-heavy modules using `PagedRequest` and `PagedResult<T>`.
- Keep route naming aligned with current plural resource patterns.
- Keep HTTP-specific request models in `SIGJ.Controllers/Models/` only when tied to ASP.NET concerns such as `IFormFile`.

## Services And Validation
- Put business validation, duplicate checks, normalization, ID generation, and orchestration in services.
- Validate active related entities in services.
- Repository validation queries must respect logical delete rules.

## Mapping
- Add or update the corresponding AutoMapper profile in `SIGJ.Services/Mappings/` when the module projects entities to response DTOs.
- Do not hide non-trivial response shaping inside services when it belongs in projection.
- If a response exposes enums or catalog-style values to clients, define their Spanish user-facing translation.
- If the technical enum value is already part of a public contract, keep it and add a `...Label` field rather than breaking compatibility.
- Avoid relying on raw `enum.ToString()` values for DTOs, exports, or other user-facing output.

## Audit Log
- Register relevant create, update, delete, reset, and assignment-sync mutations through `IAuditLogService`.
- Include table name, action, primary key, and old/new values when applicable.

## Permissions
- Add permission constants in `SIGJ.Services/Constants/Permissions.cs` when the module is permission-protected.
- If `SIGJ.Data/Scripts/20260311_SeedSetPermissions.sql` exists in the workspace, update it in the same change. If it is not present in the repository, document that the local permission seed must be updated outside version control.
- Protect controller endpoints with `HasPermission(...)`.

## Documentation Sync
- Update `docs/ai/` in the same task.
- Add or update `docs/ai/modules/<module>.md`.
- Update shared docs when the module changes cross-cutting rules, patterns, or decisions.

## Verification
- Build the solution.
- Verify affected routes, permissions, mappings, migrations, and docs.
- Add or update unit and/or integration tests when the module changes business rules, auth behavior, paging, or persistence logic.
