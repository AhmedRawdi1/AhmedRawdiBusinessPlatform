using AhmedRawdiBusinessPlatform.Services;
using Microsoft.AspNetCore.Mvc;

namespace AhmedRawdiBusinessPlatform.Controllers
{
    public class LanguageController : Controller
    {
        private readonly ILanguageService _languageService;

        public LanguageController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        [HttpGet]
        public IActionResult SetLanguage(string culture, string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                _languageService.SetCulture(culture);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
