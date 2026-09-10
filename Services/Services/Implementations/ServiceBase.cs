using Data.Entities;
using Data.Repositories.Interfaces;
using Services.Services.Interfaces;
using Services.Exceptions;

namespace Services.Services.Implementations
{
    public abstract class ServiceBase
    {
        private readonly IUserContextService _userContextService;

        protected ServiceBase(IUserContextService userContextService)
        {
            _userContextService = userContextService;
        }

        protected int GetCurrentUserId()
        {
            return _userContextService.GetCurrentUserId();
        }

        protected IReadOnlyCollection<int> GetAssignedBranchIds()
        {
            return _userContextService.GetAssignedBranchIds();
        }

        protected bool HasPermission(string permission)
        {
            return _userContextService.HasPermission(permission);
        }

        protected string GetCurrentUserName()
        {
            return _userContextService.GetCurrentUserName();
        }

        protected void SetCreationAudit(AuditableEntity entity)
        {
            var userId = GetCurrentUserId();
            var now = DateTimeOffset.UtcNow;

            entity.CreatedAt = now;
            entity.ModifiedAt = now;
            entity.CreatedByUserId = userId;
            entity.ModifiedByUserId = userId;
            entity.IsDeleted = false;
        }

        protected void SetModificationAudit(AuditableEntity entity)
        {
            entity.ModifiedAt = DateTimeOffset.UtcNow;
            entity.ModifiedByUserId = GetCurrentUserId();
        }

        protected void SetDeleted(AuditableEntity entity)
        {
            entity.IsDeleted = true;
            SetModificationAudit(entity);
        }

    }
}
