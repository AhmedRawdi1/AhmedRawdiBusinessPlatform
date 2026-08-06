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
        public IActionResult UserGroups()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Users()
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
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Json(users);
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
        public async Task<IActionResult> GetUserPermissions(long? userId, long? groupId)
        {
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
            return Json(await _groupService.GetAllSystemFormsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUser([FromForm] SaveUserDto model)
        {
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
    }
}

