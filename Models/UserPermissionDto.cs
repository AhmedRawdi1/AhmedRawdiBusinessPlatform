namespace AhmedRawdiBusinessPlatform.Models
{
    public class UserPermissionDto
    {
        public long ModuleID { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string? ModuleEnglishName { get; set; }
        public string? ModuleArabicName { get; set; }
        public long SubModuleID { get; set; }
        public string SubModuleCode { get; set; } = string.Empty;
        public string? SubModuleEnglishName { get; set; }
        public string? SubModuleArabicName { get; set; }
        public long FormID { get; set; }
        public string FormCode { get; set; } = string.Empty;
        public string? FormEnglishName { get; set; }
        public string? FormArabicName { get; set; }
        public bool CanSave { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanSearch { get; set; }
        public bool CanPrint { get; set; }
        public bool HasUserOverride { get; set; }
    }
}
