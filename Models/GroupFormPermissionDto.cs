namespace AhmedRawdiBusinessPlatform.Models
{
    public class GroupFormPermissionDto
    {
        public long? ModuleID { get; set; }
        public string? ModuleCode { get; set; }
        public string? ModuleEnglishName { get; set; }
        public string? ModuleArabicName { get; set; }
        public long SubModuleID { get; set; }
        public string SubModuleCode { get; set; } = string.Empty;
        public string SubModuleEnglishName { get; set; } = string.Empty;
        public string SubModuleArabicName { get; set; } = string.Empty;
        public long FormID { get; set; }
        public string FormCode { get; set; } = string.Empty;
        public string FormEnglishName { get; set; } = string.Empty;
        public string FormArabicName { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanSave { get; set; }
        public bool CanDelete { get; set; }
        public bool CanSearch { get; set; }
        public bool CanPrint { get; set; }
    }
}
