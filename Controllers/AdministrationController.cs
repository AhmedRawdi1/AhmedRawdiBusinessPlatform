using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AhmedRawdiBusinessPlatform.Services;
using Microsoft.Data.SqlClient;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(long? groupId)
        {
            if (!groupId.HasValue)
            {
                return BadRequest(new { success = false, code = "InvalidSelection" });
            }

            try
            {
                await _groupService.DeleteGroupAsync(groupId.Value);
                return Json(new { success = true });
            }
            catch (SqlException exception) when (exception.Number == 50002)
            {
                return NotFound(new { success = false, code = "GroupNotFound" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, code = "DeleteFailed" });
            }
        }
    }
}
