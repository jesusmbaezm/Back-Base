using System.ComponentModel.DataAnnotations;

namespace Services.DTOs.Users
{
    public class ResetUserPasswordDto
    {
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
