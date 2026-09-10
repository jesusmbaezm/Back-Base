namespace Services.DTOs.Parameters
{
    public class ParameterDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Value { get; set; }
        public DateTimeOffset ModificationDate { get; set; }
        public int ModifiedByUserId { get; set; }
    }
}
