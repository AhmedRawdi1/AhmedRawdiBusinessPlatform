namespace AhmedRawdiBusinessPlatform.Models
{
    public class GroupFormPermissionInputDto
    {
        public long FormID { get; set; }
        public bool CanView { get; set; }
        public bool CanSave { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanSearch { get; set; }
        public bool CanPrint { get; set; }
    }

    public class SaveGroupRequestDto
    {
        public long? GroupID { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string? ArabicName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public List<GroupFormPermissionInputDto> Permissions { get; set; } = new();
    }
}
