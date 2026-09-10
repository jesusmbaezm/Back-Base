namespace Data.Entities
{
    public class Parameter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Value { get; set; }
        public DateTimeOffset ModificationDate { get; set; }
        public int ModifiedByUserId { get; set; }
    }
}

