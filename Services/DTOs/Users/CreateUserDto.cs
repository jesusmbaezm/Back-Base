using System.ComponentModel.DataAnnotations;

namespace Services.DTOs.Users
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        public List<int> RoleIds { get; set; } = [];
        public List<int> BranchIds { get; set; } = [];
    }
}
