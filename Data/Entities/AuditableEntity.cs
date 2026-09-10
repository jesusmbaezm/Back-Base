namespace Data.Entities
{
    public abstract class AuditableEntity
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public int ModifiedByUserId { get; set; }
    }
}
