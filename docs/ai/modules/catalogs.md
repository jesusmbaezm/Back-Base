# Catalog Modules

## Purpose
Quick reference for the standard CRUD-style modules that follow the common repository/service/controller pattern.

## Included Modules
- Parameters
- Roles and permissions

## Main Controllers
- `SIGJ.Controllers/Controllers/ParametersController.cs`
- `SIGJ.Controllers/Controllers/RolesController.cs`

## Shared Expectations
- Standard modules usually expose `GET`, `GET paged`, `GET by id`, `POST`, `PUT`, and `DELETE`, except read-only modules such as tax regimes.
- Permission names follow the `module.read/create/update/delete` pattern.
- Paged endpoints use `PagedRequest` and `PagedResult<T>`.
- Services own validation, duplicate checks, and business formatting.

## Module-Specific Notes
