namespace AhmedRawdiBusinessPlatform.Models;

public sealed class MasterDataPageViewModel
{
    public required string EntityKey { get; init; }
    public required string EnglishTitle { get; init; }
    public required string ArabicTitle { get; init; }
    public required string EnglishSubtitle { get; init; }
    public required string ArabicSubtitle { get; init; }
    public required string IconClass { get; init; }
    public IReadOnlyList<MasterDataFieldViewModel> Fields { get; init; } = [];
}

public sealed class MasterDataFieldViewModel
{
    public required string Name { get; init; }
    public required string EnglishLabel { get; init; }
    public required string ArabicLabel { get; init; }
    public string Type { get; init; } = "text";
    public int? MaxLength { get; init; }
    public bool Required { get; init; }
    public bool FullWidth { get; init; }
    public string? EnglishPlaceholder { get; init; }
    public string? ArabicPlaceholder { get; init; }
    public IReadOnlyList<MasterDataOptionViewModel> Options { get; init; } = [];
    public string? LookupEntity { get; init; }
}

public sealed class MasterDataOptionViewModel
{
    public required string Value { get; init; }
    public required string EnglishText { get; init; }
    public required string ArabicText { get; init; }
}
