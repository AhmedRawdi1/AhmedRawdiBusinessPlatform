using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AhmedRawdiBusinessPlatform.Services;
using AhmedRawdiBusinessPlatform.Models;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace AhmedRawdiBusinessPlatform.Controllers
{
    [Authorize]
    public class AdministrationController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;

        public AdministrationController(IGroupService groupService, IUserService userService, IPermissionService permissionService)
        {
            _groupService = groupService;
            _userService = userService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> UserGroups()
        {
            if (!await CanAsync("Frm_UserGroupsManagement", "View")) return Forbid();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            if (!await CanAsync("Frm_UsersManagement", "View")) return Forbid();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGroups()
        {
            if (!await CanAsync("Frm_UserGroupsManagement", "Search") && !await CanAsync("Frm_UsersManagement", "View")) return Forbid();
            var groups = await _groupService.GetAllGroupsAsync();
            return Json(groups);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!await CanAsync("Frm_UsersManagement", "Search")) return Forbid();
            var users = await _userService.GetAllUsersAsync();
            return Json(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupPermissions(long? groupId)
        {
            if (!await CanAsync("Frm_UserGroupsManagement", "Search")) return Forbid();
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
        public async Task<IActionResult> GetUserPermissions(long? userId, long? groupId)
        {
            if (!await CanAsync("Frm_UsersManagement", "Search")) return Forbid();
            if (!userId.HasValue && !groupId.HasValue)
            {
                return BadRequest(new { success = false, code = "InvalidSelection" });
            }

            try
            {
                var permissions = await _permissionService.GetUserPermissionsAsync(userId, groupId);
                return Json(permissions);
            }
            catch (SqlException exception) when (exception.Number == 50011)
            {
                return NotFound(new { success = false, code = "GroupNotFound" });
            }
            catch (SqlException exception) when (exception.Number == 50012)
            {
                return NotFound(new { success = false, code = "UserNotFound" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSystemForms()
        {
            if (!await CanAsync("Frm_UserGroupsManagement", "View")) return Forbid();
            return Json(await _groupService.GetAllSystemFormsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGroup([FromForm] SaveGroupDto model)
        {
            if (!await CanAsync("Frm_UserGroupsManagement", model.GroupID.HasValue ? "Update" : "Save")) return Forbid();
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, code = "InvalidData" });
            }

            try
            {
                long? regBy = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var currentUserId))
                {
                    regBy = currentUserId;
                }

                var groupId = await _groupService.SaveGroupAsync(model, regBy);
                return Json(new { success = true, groupId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, code = "SaveFailed", message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUser([FromForm] SaveUserDto model)
        {
            if (!await CanAsync("Frm_UsersManagement", model.UserID.HasValue ? "Update" : "Save")) return Forbid();
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, code = "InvalidData" });
            }

            try
            {
                long? regBy = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var currentUserId))
                {
                    regBy = currentUserId;
                }

                var userId = await _userService.SaveUserAsync(model, regBy);
                return Json(new { success = true, userId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, code = "SaveFailed", message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(long? groupId)
        {
            if (!await CanAsync("Frm_UserGroupsManagement", "Delete")) return Forbid();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(long? userId)
        {
            if (!await CanAsync("Frm_UsersManagement", "Delete")) return Forbid();
            if (!userId.HasValue)
            {
                return BadRequest(new { success = false, code = "InvalidSelection" });
            }

            try
            {
                await _userService.DeleteUserAsync(userId.Value);
                return Json(new { success = true });
            }
            catch (SqlException exception) when (exception.Number == 50002)
            {
                return NotFound(new { success = false, code = "UserNotFound" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, code = "DeleteFailed" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword([FromForm] ResetPasswordDto model)
        {
            if (!User.HasClaim("IsSystemOwner", bool.TrueString)) return Forbid();
            if (!ModelState.IsValid) return BadRequest(new { success=false, code="InvalidPassword" });
            if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ownerId)) return Forbid();

            try
            {
                await _userService.ResetPasswordAsync(model.UserID, model.NewPassword, ownerId);
                return Json(new { success=true });
            }
            catch (Exception exception) when (exception is ArgumentException or SqlException)
            {
                return BadRequest(new { success=false, message=exception.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUserPermissions(long userId, long? groupId, string permissionsJson)
        {
            if (!await CanAsync("Frm_UsersManagement", "Update")) return Forbid();
            if (userId <= 0)
            {
                return BadRequest(new { success = false, code = "InvalidSelection" });
            }

            try
            {
                long? regBy = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var currentUserId))
                {
                    regBy = currentUserId;
                }

                await _permissionService.SaveUserPermissionsAsync(userId, groupId, permissionsJson, regBy);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = ex.Message });
            }
        }

        private long? CurrentUserId() => long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        private long? CurrentGroupId() => long.TryParse(User.FindFirstValue("GroupID"), out var id) ? id : null;
        private Task<bool> CanAsync(string formCode, string permission) =>
            _permissionService.HasFormPermissionAsync(CurrentUserId(), CurrentGroupId(), formCode, permission);
    }
}

