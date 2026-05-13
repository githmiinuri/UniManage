using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.ViewModels;
using UniManage3.Models;
using System.Security.Claims;

namespace UniManage3.Controllers
{
    [Authorize(Roles = "Student")]
    public class MyCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MyCoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return View("Error");

            // Fetch Enrolled Courses with hierarchical Semester and Module data
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Semesters)
                        .ThenInclude(s => s.Modules)
                            .ThenInclude(m => m.Lecturer)
                .ToListAsync();

            var viewModel = new MyCoursesViewModel
            {
                EnrolledCourses = enrollments.Select(e => new EnrolledCourseViewModel
                {
                    CourseId = e.CourseId,
                    CourseCode = e.Course.CourseCode,
                    CourseName = e.Course.CourseName,
                    Semesters = e.Course.Semesters.OrderBy(s => s.SemesterNumber).Select(s => new SemesterGroupViewModel
                    {
                        SemesterId = s.Id,
                        SemesterName = s.SemesterName,
                        SemesterNumber = s.SemesterNumber,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        Modules = s.Modules.Select(m => new ModuleDetailViewModel
                        {
                            ModuleId = m.Id,
                            ModuleCode = m.ModuleCode,
                            ModuleName = m.ModuleName,
                            Description = m.Description,
                            Credits = m.Credits,
                            LecturerFullName = m.Lecturer != null ? $"{m.Lecturer.FirstName} {m.Lecturer.LastName}" : "Not Assigned"
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ModuleContent(int id)
        {
            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            var isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == student.Id && e.CourseId == module.CourseId);
            if (!isEnrolled) return Forbid();

            var materials = await _context.CourseMaterials
                .Where(cm => cm.ModuleId == id)
                .ToListAsync();

            ViewBag.ModuleName = module.ModuleName;
            ViewBag.ModuleCode = module.ModuleCode;
            ViewBag.CourseName = module.Course.CourseName;

            return View(materials);
        }

        public async Task<IActionResult> DownloadMaterial(int id)
        {
            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null || string.IsNullOrEmpty(material.FilePath))
                return NotFound("Material not found.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", material.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
                return NotFound("Physical file not found on server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = material.MaterialName + Path.GetExtension(filePath);

            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}