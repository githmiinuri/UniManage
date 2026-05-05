using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UniManage3.Models;

namespace UniManage3.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrator,Lecturer,Student")]
        public IActionResult Index()
        {
            // Check the user's role and send them to their specific dashboard
            if (User.IsInRole("Administrator"))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (User.IsInRole("Lecturer"))
            {
                return RedirectToAction("Index", "LectureDashboard");
            }

            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Dashboard", "Student");
            }

            return View();
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
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
