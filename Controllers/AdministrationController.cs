using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AhmedRawdiBusinessPlatform.Controllers
{
    [Authorize]
    public class AdministrationController : Controller
    {
        [HttpGet]
        public IActionResult UserGroups()
        {
            return View();
        }
    }
}
