using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Models;
using AhmedRawdiBusinessPlatform.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AhmedRawdiBusinessPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AccountController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
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
        [EnableRateLimiting("login")]
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
                ,new Claim("IsSystemOwner", userInfo.IsSystemOwner.ToString())
                ,new Claim("MustChangePassword", userInfo.MustChangePassword.ToString())
                ,new Claim("SecurityStamp", userInfo.SecurityStamp.ToString())
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

            return userInfo.MustChangePassword
                ? RedirectToAction(nameof(ChangePassword))
                : RedirectToLocal(returnUrl);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var userCode = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var verification = await _authService.ValidateUserAsync(userCode, model.CurrentPassword);
            if (!verification.IsSuccess || !long.TryParse(userIdText, out var userId))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "The current password is incorrect.");
                return View(model);
            }

            try
            {
                await _userService.ChangeOwnPasswordAsync(userId, model.NewPassword);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["PasswordChanged"] = "Password changed successfully. Please sign in again.";
                return RedirectToAction(nameof(Login));
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(nameof(model.NewPassword), exception.Message);
                return View(model);
            }
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
