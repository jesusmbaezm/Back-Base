# Project Overview

## Purpose
SIGJ is an ASP.NET Core API organized as a three-layer .NET 8 solution.

## Solution Layout
- `Controllers/`: API entrypoint, controllers, middleware, HTTP-specific request models
- `Services/`: business services, DTOs, mappings, validation helpers, business exceptions
- `Data/`: EF Core context, entities, repositories, configurations, migrations, SQL scripts

## Active Modules
- Authentication
- Users
- Roles and permissions
- Parameters

## Standard Commands
- Restore: `dotnet restore DEMO.sln`
- Build: `dotnet build DEMO.sln -c Debug`
- Run API: `dotnet run --project Controllers`
- Hot reload: `dotnet watch run --project Controllers`
- Add migration from terminal: `dotnet ef migrations add MigrationName --project Data`
- Apply migration from terminal: `dotnet ef database update --project Data`

## Current Docs Strategy
- `AGENTS.md` is the short entry-point for agents.
- `docs/ai/` contains the durable project context, rules, and module guides.
- Update the relevant docs whenever code changes behavior or reveals a mismatch.
