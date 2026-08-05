using System;

namespace AhmedRawdiBusinessPlatform.Models
{
    public class GroupMemberDto
    {
        public long UserID { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string? UserEnglishName { get; set; }
        public string? UserArabicName { get; set; }
        public string? Email { get; set; }
        public string? MobileNum { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RegDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public long GroupID { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupEnglishName { get; set; }
        public string? GroupArabicName { get; set; }
    }
}
