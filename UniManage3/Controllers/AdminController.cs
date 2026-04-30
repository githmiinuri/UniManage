using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniManage3.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            // Render the renamed Admin view (Views/Admin/Admin.cshtml)
            return View("Admin");
        }

        // Diagnostic/plain admin page that does not use the shared layout.
        public IActionResult Plain()
        {
            return View("AdminPlain");
        }
    }
}
