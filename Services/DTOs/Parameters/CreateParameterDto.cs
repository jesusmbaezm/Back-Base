using System.ComponentModel.DataAnnotations;

namespace Services.DTOs.Parameters
{
    public class CreateParameterDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;
    }
}
