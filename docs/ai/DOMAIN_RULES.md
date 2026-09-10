# Domain Rules

## Purpose
Central source for cross-module business and persistence invariants.

## Logical Delete And Audit
- User-managed tables use `IsDeleted`, `CreatedAt`, `ModifiedAt`, `CreatedByUserId`, and `ModifiedByUserId`.
- Deletes are logical by default.
- List and get-by-id flows exclude logically deleted rows unless a requirement explicitly says otherwise.
- Audit fields are populated in services, not controllers.

## Business Identifiers


## User Assignment Rules
- `Users` is an auditable, logically deleted table.
- User standard reads and get-by-id flows must exclude `IsDeleted = true`.
- User auth lookups must require both `!IsDeleted` and `IsActive`.
- User delete sets `IsDeleted = true` and also forces `IsActive = false`.
- User email uniqueness applies only to non-deleted users, so emails from deleted users may be reused.
- Assignments are managed from the user module.
- Login responses expose active assigned branches.
- User-scoped branch reads must rehydrate assigned branch ids through active branch repository queries before returning branch data.
- User responses should load assigned branches through repository includes and AutoMapper projection.

## Enum Persistence
- Use enums under `Data/Enums/` for persisted catalog-style values.
- Persist readable enum values with explicit `HasConversion<string>()` when applicable.
- Persisted enums and technical enum names remain in English.
- Client-visible enum output must not rely on raw `enum.ToString()` values as the user-facing representation.
- When an enum is returned to clients, expose a Spanish business label for UI, exports, or user-facing messages.
- If the technical enum value is already part of the public contract, preserve it and add a companion label field such as `...Label` instead of replacing the technical field.


## Permission Seed Handling
- `Services/Constants/Permissions.cs` is the required in-repo source for permission names.
- `Data/Scripts/20260311_SeedSetPermissions.sql` may exist only in local workspaces.
- When that SQL seed file exists in the workspace, keep it synchronized with permission changes.
- When it is not present in the repository, document the required local seed update instead of failing the task.
