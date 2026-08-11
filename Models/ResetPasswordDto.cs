using System.ComponentModel.DataAnnotations;

namespace AhmedRawdiBusinessPlatform.Models;

public sealed class ResetPasswordDto
{
    [Range(1, long.MaxValue)]
    public long UserID { get; set; }

    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
