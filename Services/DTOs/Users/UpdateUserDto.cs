using System.ComponentModel.DataAnnotations;

namespace Services.DTOs.Users
{
    public class UpdateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public List<int> RoleIds { get; set; } = [];
        public List<int> BranchIds { get; set; } = [];
    }
}
