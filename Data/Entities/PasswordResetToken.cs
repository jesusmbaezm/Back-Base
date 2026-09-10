namespace Data.Entities
{
    public class PasswordResetToken : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string TokenHash { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? UsedAt { get; set; }

        public User? User { get; set; }
    }
}
