namespace AhmedRawdiBusinessPlatform.Models
{
    public class SaveGroupDto
    {
        public long? GroupID { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string? ArabicName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? PermissionsJson { get; set; }
    }
}
