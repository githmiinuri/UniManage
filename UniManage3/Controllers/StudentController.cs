using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace UniManage3.Controllers
{
    [Authorize] // Ensures only logged-in users can access
    public class StudentController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        
        public IActionResult BrowseCourses() => View();
        public IActionResult MyCourses() => View();
        public IActionResult Assignments() => View();
        public IActionResult Grades() => View();
        public IActionResult Calendar() => View();
        public IActionResult Library() => View();
    }
}
