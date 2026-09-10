namespace Services.DTOs.Common
{
    public class AuditableResponseDto
    {
        public int CreatedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int ModifiedByUserId { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
