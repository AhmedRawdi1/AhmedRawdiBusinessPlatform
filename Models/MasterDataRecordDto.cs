namespace AhmedRawdiBusinessPlatform.Models;

public sealed class MasterDataRecordDto
{
    public long ID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
