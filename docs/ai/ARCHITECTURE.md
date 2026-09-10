# Architecture

## Purpose
Defines where code belongs and how the three solution layers interact.

## Layer Responsibilities
- `Controllers`: routing, HTTP models, auth attributes, response helpers, multipart request handling
- `Services`: business rules, orchestration, validation, normalization, audit log calls, DTO contracts, AutoMapper profiles
- `Data`: entities, EF configurations, repositories, migrations, SQL seed scripts

## Dependency Flow
- Controllers depend on services.
- Services depend on data.
- Data does not depend on services or controllers.

## DTO Placement
- HTTP-specific request models that depend on ASP.NET concerns such as `IFormFile` belong in `Controllers/Models/`.
- Business DTOs belong in `Services/DTOs/`.

## Projection Strategy
- Use AutoMapper profiles in `Services/Mappings/` for entity-to-response projection.
- Keep non-trivial create, update, normalization, and validation rules in services.

## Repository Expectations
- Repositories own query composition, filtering, paging, sorting, and include shaping.
- Services own orchestration and business validation.
- Logical delete filters should be applied in repository queries unless a requirement explicitly says otherwise.

## Mutation Flow
- Validate request and related entities in the service.
- Apply audit field updates in the service.
- Persist through repositories / DbContext.
- Register audit logs through `IAuditLogService` for relevant business mutations.
