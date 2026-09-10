using Services.DTOs.Users;

namespace Services.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<string> Permissions { get; set; }
        public IEnumerable<UserBranchDto> Branches { get; set; } = [];
    }
}
