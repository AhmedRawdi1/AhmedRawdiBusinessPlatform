using System.Security.Claims;
using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Services;
using Microsoft.AspNetCore.Mvc;

namespace AhmedRawdiBusinessPlatform.ViewComponents
{
    public class NavigationMenuViewComponent : ViewComponent
    {
        private readonly IPermissionService _permissionService;

        public NavigationMenuViewComponent(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            long? userId = null;
            long? groupId = null;

            if (UserClaimsPrincipal.Identity != null && UserClaimsPrincipal.Identity.IsAuthenticated)
            {
                var userIdClaim = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                var groupIdClaim = UserClaimsPrincipal.FindFirst("GroupID")?.Value;
                if (long.TryParse(groupIdClaim, out var parsedGroupId))
                {
                    groupId = parsedGroupId;
                }
            }
            else
            {
                // Fallback for unauthenticated/demo state to showcase stored procedure results (Admin UserID = 0)
                userId = 0;
            }

            var navigationMenu = await _permissionService.GetNavigationMenuAsync(userId, groupId);
            return View(navigationMenu);
        }
    }
}
