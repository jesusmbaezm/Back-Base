namespace Services.DTOs.Users
{
    public class UserBranchDto
    {
        public int Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsMain { get; set; }
    }
}
