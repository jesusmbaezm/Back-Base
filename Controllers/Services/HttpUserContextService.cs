using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Services.Exceptions;
using Services.Services.Interfaces;

namespace Controllers.Services
{
    public class HttpUserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpUserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdValue = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue("sub");

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new ValidationException("El id del usuario autenticado es invalido.");
            }

            return userId;
        }

        public IReadOnlyCollection<int> GetAssignedBranchIds()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null)
            {
                return [];
            }

            return user.FindAll("branch_id")
                .Select(claim => int.TryParse(claim.Value, out var branchId) ? branchId : (int?)null)
                .Where(branchId => branchId.HasValue)
                .Select(branchId => branchId!.Value)
                .Distinct()
                .ToList();
        }

        public bool HasPermission(string permission)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.Any(c => c.Type == "permission" && c.Value == permission) ?? false;
        }

        public string GetCurrentUserName()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null) return string.Empty;

            return user.FindFirstValue("name")
                ?? user.FindFirstValue(ClaimTypes.Name)
                ?? user.FindFirstValue("unique_name")
                ?? user.FindFirstValue(ClaimTypes.GivenName)
                ?? string.Empty;
        }
    }
}
