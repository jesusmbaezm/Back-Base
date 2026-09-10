# Repository Guidelines

Read this file first. Then open only the AI docs needed for the area you will modify.

## Core Rules
- Keep all code identifiers, DTOs, entities, enums, controllers, repositories, and persisted names in English.
- Preserve the dependency flow `Controllers` -> `Services` -> `Data`.
- Keep controllers thin; business rules, validation, audit orchestration, and normalization belong in services.
- Use AutoMapper profiles in `Services/Mappings/` for response projection.
- User-managed tables use logical delete and audit fields. Filter `IsDeleted = true` out of list and get-by-id flows unless a requirement explicitly says otherwise.
- New permission-protected modules must update `Services/Constants/Permissions.cs`. update it in the same change; otherwise document that the local permission seed must be updated outside the repository.


## Documentation Sync Rule
Documentation is part of the deliverable.

- Every change that affects behavior, architecture, API shape, persistence, permissions, mappings, audit flow, branch rules, auth payloads, or module responsibilities must update the relevant `.md` files under `docs/ai/`.
- If code and documentation do not match, verify the implemented behavior and update the affected docs to reflect the real current state unless the task explicitly aims to change the code to match the docs.
- Treat documentation sync as part of task completion criteria.
- New modules must update the shared docs they affect and create or update their file under `docs/ai/modules/`.

## Read Next By Topic
- Fast project facts: `docs/ai/context.yaml`
- AI docs map: `docs/ai/README.md`
- Project and commands: `docs/ai/PROJECT_OVERVIEW.md`
- Layer responsibilities: `docs/ai/ARCHITECTURE.md`
- Cross-module business rules: `docs/ai/DOMAIN_RULES.md`
- Required checklist for new modules: `docs/ai/MODULE_CHECKLIST.md`
- Shared implementation patterns: `docs/ai/PATTERNS.md`
- Architectural decisions: `docs/ai/DECISION_LOG.md`
- Auth and JWT: `docs/ai/modules/auth.md`
- Users assignments: `docs/ai/modules/users.md`
- Parameters, roles: `docs/ai/modules/catalogs.md`

## High-Value Current Facts

