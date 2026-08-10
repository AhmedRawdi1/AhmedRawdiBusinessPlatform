using System;
using System.ComponentModel.DataAnnotations;

namespace AhmedRawdiBusinessPlatform.Models
{
    public class SaveUserDto
    {
        public long? UserID { get; set; }
        [Range(1, long.MaxValue)]
        public long GroupID { get; set; }
        [Required, StringLength(100)]
        public string Code { get; set; } = string.Empty;
        [Required, StringLength(150)]
        public string EngName { get; set; } = string.Empty;
        [StringLength(150)]
        public string? ArbName { get; set; }
        [EmailAddress, StringLength(200)]
        public string? Email { get; set; }
        [Phone, StringLength(50)]
        public string? MobileNum { get; set; }
        [RegularExpression("^(en-US|ar-SA)$")]
        public string? PreferredLanguage { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiredDate { get; set; }
        [MinLength(8), StringLength(256)]
        public string? UserPass { get; set; }
    }
}
