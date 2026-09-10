# Auth Module

## Purpose
Authentication and JWT issuance.

## When To Read
Read when changing login behavior, JWT claims, or auth response payloads.

## Main Entry Points
- Controller: `SIGJ.Controllers/Controllers/AuthController.cs`
- Service: `SIGJ.Services/Services/Implementations/AuthService.cs`
- DTOs: `SIGJ.Services/DTOs/Auth/`

## Current Behavior
- Login endpoint is `POST api/auth/login`.
- Token refresh endpoint is `POST api/auth/refresh` (requires `[Authorize]`).
- Password recovery endpoints are `POST api/auth/forgot-password` and `POST api/auth/reset-password`.
- Authentication validates email and password, then loads roles, permissions, and assigned branches.
- Login and password recovery ignore logically deleted users in the same way they ignore inactive users.
- Login response includes token metadata, roles, permissions, and active assigned branches.
- JWT contains standard user claims, role claims, `permission` claims.
- Forgot-password always returns a generic success response, even when the email does not exist, the user is inactive, or the user is logically deleted.
- Password recovery stores one active one-time token per user in `PasswordResetTokens`, sends the code through SMTP via MailKit, and marks the token as used after a successful reset.
- SMTP settings for password recovery are loaded from `Parameters` instead of `appsettings`; missing, invalid, or transport/authentication SMTP errors are logged without changing the generic forgot-password response.
- Deployment order for recovery by email is: apply the schema migration, run `SIGJ.Data/Scripts/20260409_SeedSmtpParameters.sql`, load the real `SmtpPassword` manually, and then test `POST api/auth/forgot-password`.

## Token Refresh
- Endpoint: `POST api/auth/refresh` — requires a valid (non-expired) JWT in the `Authorization: Bearer` header.
- Uses sliding-window renewal: the token is only eligible for refresh during the last `RefreshWindowMinutes` (default 10) before expiry.
- No refresh tokens stored in DB — the current JWT is validated and a new one is issued with fresh claims (roles, permissions, branches reloaded from DB).
- Rejects tokens that are already expired or that still have more than `RefreshWindowMinutes` remaining.
- Configuration: `JwtSettings.RefreshWindowMinutes` in `appsettings.json` (default: 10).
- Response: `RefreshTokenResponseDto { Token, ExpiresAt }` — no user metadata (frontend already has it from login).

## Important Rules
- Active branch claims come from `UserBranches` filtered by assignment state and active branch state.
- Token refresh reloads all claims from DB — if a user's roles, permissions, or branches changed since login, the refreshed token reflects the current state.
- If auth payloads change, update both this file and `docs/ai/DOMAIN_RULES.md` when the change affects cross-module behavior.
