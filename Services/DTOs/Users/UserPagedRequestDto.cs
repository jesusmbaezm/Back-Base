using Services.DTOs.Shared;

namespace Services.DTOs.Users
{
    public class UserPagedRequestDto : PagedRequest
    {
        public int? RoleId { get; set; }
        public bool? IsActive { get; set; }
    }
}
