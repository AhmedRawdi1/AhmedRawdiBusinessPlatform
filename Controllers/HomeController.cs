using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Models;
using AhmedRawdiBusinessPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AhmedRawdiBusinessPlatform.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPermissionService _permissionService;

        public HomeController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            long? userId = null;
            long? groupId = null;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                var groupIdClaim = User.FindFirst("GroupID")?.Value;
                if (long.TryParse(groupIdClaim, out var parsedGroupId))
                {
                    groupId = parsedGroupId;
                }
            }
            else
            {
                userId = 0;
            }

            var navMenu = await _permissionService.GetNavigationMenuAsync(userId, groupId);
            ViewBag.NavMenu = navMenu;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
