using System.ComponentModel.DataAnnotations;

namespace AhmedRawdiBusinessPlatform.Models
{
    public class SaveGroupDto
    {
        public long? GroupID { get; set; }
        [Required, StringLength(20)]
        public string GroupCode { get; set; } = string.Empty;
        [Required, StringLength(150)]
        public string EnglishName { get; set; } = string.Empty;
        [StringLength(150)]
        public string? ArabicName { get; set; }
        [StringLength(1000)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? PermissionsJson { get; set; }
    }
}
