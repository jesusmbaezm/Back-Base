# Entity Quick Reference — SIGJ

Cross-module entities with their full field list. Use this instead of reading entity files when writing services, repositories, or mappers.

All auditable entities inherit `AuditableEntity`: `IsDeleted`, `CreatedAt`, `ModifiedAt`, `CreatedByUserId`, `ModifiedByUserId`.

---

## User
`SIGJ.Data/Entities/User.cs` - inherits `AuditableEntity`

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `Name` | `string` | |
| `Email` | `string` | |
| `PasswordHash` | `string` | |
| `IsActive` | `bool` | |

**Nav**: `UserRoles[]`, `UserBranches[]`, `PasswordResetTokens[]`

