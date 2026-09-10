using System.ComponentModel.DataAnnotations;

namespace Services.DTOs.Roles
{
    public class RoleRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public List<int> PermissionIds { get; set; } = [];
    }
}
