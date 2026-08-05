using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AhmedRawdiBusinessPlatform.Services;

namespace AhmedRawdiBusinessPlatform.Controllers
{
    [Authorize]
    public class AdministrationController : Controller
    {
        private readonly IGroupService _groupService;

        public AdministrationController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet]
        public IActionResult UserGroups()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await _groupService.GetAllGroupsAsync();
            return Json(groups);
        }
    }
}
