using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Models;
using AhmedRawdiBusinessPlatform.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AhmedRawdiBusinessPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (isSuccess, message, userInfo) = await _authService.ValidateUserAsync(model.UserCode, model.Password);

            if (!isSuccess || userInfo == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.UserID.ToString()),
                new Claim(ClaimTypes.Name, userInfo.UserCode),
                new Claim("UserEnglishName", userInfo.UserEnglishName ?? string.Empty),
                new Claim("UserArabicName", userInfo.UserArabicName ?? string.Empty),
                new Claim(ClaimTypes.Email, userInfo.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, userInfo.GroupEnglishName ?? userInfo.GroupCode ?? "User"),
                new Claim("GroupID", userInfo.GroupID.ToString()),
                new Claim("GroupCode", userInfo.GroupCode ?? string.Empty)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
