namespace Services.DTOs.Parameters
{
    public class ParameterFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Name { get; set; }
        public string? Type { get; set; }
    }
}
