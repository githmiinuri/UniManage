using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace UniManage3.Controllers
{
    [Authorize] // Ensures only logged-in users can access
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> BrowseCourses()
        {
            // Fetch related data and build the view model so the view never receives a null Model
            var departments = await _context.Departments.ToListAsync();
            var allCourses = await _context.Courses.ToListAsync();
            var allModules = await _context.Modules.ToListAsync();
            var allSemesters = await _context.Semesters.ToListAsync();

            var viewModel = new BrowseCoursesViewModel
            {
                Departments = departments.Select(d => new DepartmentGroupViewModel
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description,
                    Courses = allCourses.Where(c => c.DepartmentId == d.Id).Select(c => new CourseCardViewModel
                    {
                        CourseId = c.Id,
                        CourseCode = c.CourseCode,
                        CourseName = c.CourseName,
                        Description = c.Description,
                        Credits = c.Credits,
                        Duration = c.Duration,
                        ModuleCount = allModules.Count(m => m.CourseId == c.Id),
                        SemesterCount = allSemesters.Count(s => s.CourseId == c.Id),
                        Modules = allModules.Where(m => m.CourseId == c.Id).Select(m => new ModuleInfoViewModel
                        {
                            ModuleCode = m.ModuleCode,
                            ModuleName = m.ModuleName,
                            Description = m.Description,
                            Credits = m.Credits
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        public IActionResult MyCourses() => RedirectToAction("Index", "MyCourses", new { area = "" });
        public IActionResult Assignments() => View();
        public IActionResult Grades() => View();
        public IActionResult Calendar() => View();
        public IActionResult Library() => View();
    }
}
