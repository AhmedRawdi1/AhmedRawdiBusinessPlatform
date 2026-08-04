namespace AhmedRawdiBusinessPlatform.Services
{
    public interface ILanguageService
    {
        string CurrentCulture { get; }
        bool IsRightToLeft { get; }
        string Get(string key);
        void SetCulture(string culture);
    }
}
