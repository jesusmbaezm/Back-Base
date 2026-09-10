# Users Module

## Purpose
Manage users, roles assigned to users, password reset flow, and user-branch assignments.

## When To Read
Read when changing user CRUD, password reset, user response shape, or branch assignment behavior.

## Main Entry Points
- Controller: `SIGJ.Controllers/Controllers/UsersController.cs`
- Service: `SIGJ.Services/Services/Implementations/UserManagementService.cs`
- Repository: `SIGJ.Data/Repositories/Implementations/UserRepository.cs`
- Mapping: `SIGJ.Services/Mappings/UserProfile.cs`
s
## Current API Shape
- `GET api/users`
- `GET api/users/paged` - accepts optional `roleId` and `isActive` filters in addition to the shared paged query params
- `GET api/users/{id}`
- `POST api/users`
- `PUT api/users/{id}`
- `DELETE api/users/{id}`
- `POST api/users/{id}/reset-password`

## Current Behavior
- `CreateUserDto` and `UpdateUserDto` accept `RoleIds` and `BranchIds`.
- `UserDto` returns roles and assigned branches.
- Users now follow the standard audit model with `IsDeleted`, `CreatedAt`, `ModifiedAt`, `CreatedByUserId`, and `ModifiedByUserId`.
- Branch assignments are synchronized in the user service using logical delete/reactivation semantics.
- Branch assignments may also be created automatically by the branches module when a new branch is created; every active user with role `Admin` receives that branch through the same logical delete/reactivation semantics.
- `DELETE api/users/{id}` performs logical delete: it sets `IsDeleted = true` and `IsActive = false`, but rejects users that still have any role with `IsSystem = true` (for example, the system admin role).
- Standard user-management reads (`GET all`, `GET paged`, `GET by id`) exclude logically deleted users but still show inactive non-deleted users.
- `GET api/users/paged` can optionally filter by a single assigned role (`roleId`) and by active status (`isActive`); both filters are optional and still exclude logically deleted users.
- User repository loads roles and branches for user detail and auth flows, always excluding deleted users from standard reads.
- `POST api/users/{id}/reset-password` is the administrative reset endpoint used by the panel and still requires `users.update`.
- Public self-service recovery is not implemented here; it lives under the auth module.
- Email uniqueness is enforced only for non-deleted users, so a deleted user's email may be reused by a new user.

## Important Rules
- Assigned branches should be projected through AutoMapper, not manually shaped in services when mapping is sufficient.
- User mutations that materially change state should continue to use audit logging.
