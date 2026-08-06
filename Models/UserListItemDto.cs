using System;

namespace AhmedRawdiBusinessPlatform.Models
{
    public class UserListItemDto
    {
        public long UserID { get; set; }
        public long GroupID { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string UserEnglishName { get; set; } = string.Empty;
        public string? UserArabicName { get; set; }
        public string? Email { get; set; }
        public string? MobileNum { get; set; }
        public string? PreferredLanguage { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RegDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string GroupEnglishName { get; set; } = string.Empty;
        public string? GroupArabicName { get; set; }
    }
}
