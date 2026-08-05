using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AhmedRawdiBusinessPlatform.Services;
using AhmedRawdiBusinessPlatform.Models;
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

        [HttpGet]
        public async Task<IActionResult> GetGroupPermissions(long? groupId)
        {
            if (!groupId.HasValue)
            {
                return BadRequest(new { success = false, code = "InvalidSelection" });
            }

            try
            {
                return Json(await _groupService.GetGroupPermissionsAsync(groupId.Value));
            }
            catch (SqlException exception) when (exception.Number == 50002)
            {
                return NotFound(new { success = false, code = "GroupNotFound" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupMembers(long? groupId)
        {
            if (!groupId.HasValue)
            {
                return BadRequest(new { success = false, code = "InvalidSelection" });
            }

            try
            {
                return Json(await _groupService.GetGroupMembersAsync(groupId.Value));
            }
            catch (SqlException exception) when (exception.Number == 50002)
            {
                return NotFound(new { success = false, code = "GroupNotFound" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSystemForms()
        {
            return Json(await _groupService.GetAllSystemFormsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGroup([FromBody] SaveGroupRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.EnglishName))
            {
                return BadRequest(new { success = false, message = "Group Code and English Name are required." });
            }

            try
            {
                var currentUserIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                long currentUserId = long.TryParse(currentUserIdString, out var parsedId) ? parsedId : 1;

                var savedGroupId = await _groupService.SaveGroupWithPermissionsAsync(request, currentUserId);
                return Json(new
                {
                    success = true,
                    groupId = savedGroupId,
                    isUpdate = request.GroupID.HasValue && request.GroupID.Value > 0
                });
            }
            catch (SqlException ex) when (ex.Number == 50004)
            {
                return BadRequest(new { success = false, message = "A system group with the same code already exists." });
            }
            catch (SqlException ex) when (ex.Number == 50005)
            {
                return NotFound(new { success = false, message = "The system group to update was not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = ex.Message });
            }
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
