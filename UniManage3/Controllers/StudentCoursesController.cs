using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.ViewModels;
using System.Security.Claims;
using UniManage3.Models;
using Microsoft.AspNetCore.Authorization;

namespace UniManage3.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentCoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudentCourses/Browse
        public async Task<IActionResult> Browse()
        {
            // Fetch data manually to avoid issues if navigation properties (like Department.Courses) are not defined in Models
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "User session not found." });
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                return Json(new { success = false, message = "Student profile not found." });
            }

            // Check if already enrolled to prevent duplicates
            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == courseId);

            if (existing != null)
            {
                return Json(new { success = false, message = "You have already applied for this course." });
            }

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = courseId,
                Status = "In-Progress",
                EnrollmentDate = DateTime.Now
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
