using System;

namespace AhmedRawdiBusinessPlatform.Models
{
    public class SaveUserDto
    {
        public long? UserID { get; set; }
        public long GroupID { get; set; }
        public string Code { get; set; } = string.Empty;
        public string EngName { get; set; } = string.Empty;
        public string? ArbName { get; set; }
        public string? Email { get; set; }
        public string? MobileNum { get; set; }
        public string? PreferredLanguage { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiredDate { get; set; }
        public string? UserPass { get; set; }
    }
}
