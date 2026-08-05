namespace AhmedRawdiBusinessPlatform.Models
{
    public class GroupListItemDto
    {
        public long GroupID { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string GroupEnglishName { get; set; } = string.Empty;
        public string GroupArabicName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime RegDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
