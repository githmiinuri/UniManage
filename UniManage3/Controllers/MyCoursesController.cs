using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.ViewModels;
using UniManage3.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace UniManage3.Controllers
{
    [Authorize(Roles = "Student")]
    public class MyCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MyCoursesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            // Identify logged in user. Prefer email (User.Identity.Name) and fallback to NameIdentifier claim.
            string? email = User?.Identity?.Name;
            User? appUser = null;

            if (!string.IsNullOrEmpty(email))
            {
                appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }

            if (appUser == null)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

                // Try numeric id first, otherwise treat the claim as an email and lookup by email.
                if (int.TryParse(userIdClaim, out var parsedUserId))
                {
                    appUser = await _context.Users.FindAsync(parsedUserId);
                }
                else
                {
                    // Sometimes NameIdentifier contains the email or provider key; try email lookup
                    appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userIdClaim);
                }
            }

            if (appUser == null) return RedirectToAction("Login", "Account");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == appUser.Id);

            // If not found by UserId, try resolving by linked user email (some setups link differently)
            if (student == null && !string.IsNullOrEmpty(email))
            {
                student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.User != null && s.User.Email == email);
            }

            if (student == null) return View("Error");

            // Fetch Enrolled Courses (load by student then filter statuses robustly to avoid DB string nuances)
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Semesters)
                        .ThenInclude(s => s.Modules)
                            .ThenInclude(m => m.Lecturer)
                .ToListAsync();

            // Filter in-memory for allowed statuses (trim + case-insensitive) to avoid mismatches due to spacing/casing
            var allowedStatuses = new[] { "In-Progress", "Completed" };
            var filtered = enrollments.Where(e =>
            {
                var st = (e.Status ?? string.Empty).Trim();
                return allowedStatuses.Any(a => string.Equals(a, st, System.StringComparison.OrdinalIgnoreCase));
            }).ToList();

            var viewModel = new MyCoursesViewModel
            {
                EnrolledCourses = filtered.Select(e => new EnrolledCourseViewModel
                {
                    CourseId = e.CourseId,
                    CourseCode = e.Course.CourseCode,
                    CourseName = e.Course.CourseName,
                    Status = e.Status,
                    EnrollmentDate = e.EnrollmentDate,
                    Grade = string.IsNullOrEmpty(e.Grade) ? "Pending" : e.Grade,
                    CompletionDate = e.CompletionDate,
                    CompletionDisplay = e.CompletionDate?.ToString("MMM dd, yyyy") ?? "TBD",
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

            // The student-facing view lives under Views/Student/MyCourses.cshtml
            return View("~/Views/Student/MyCourses.cshtml", viewModel);
        }

        // Redirect-friendly action so student pages can link to the main browse catalog
        public IActionResult BrowseCourses()
        {
            // The main course catalog lives in StudentCoursesController.Browse
            return RedirectToAction("Browse", "StudentCourses");
        }

        public async Task<IActionResult> ModuleContent(int id)
        {
            // Load the module and its parent course explicitly to avoid null refs when rendering view data
            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null) return RedirectToAction("Index");

            // Identify logged in user (email preferred)
            string? email = User?.Identity?.Name;
            User? appUser = null;

            if (!string.IsNullOrEmpty(email))
            {
                appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }

            if (appUser == null)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");
                if (!int.TryParse(userIdClaim, out var parsedUserId)) return RedirectToAction("Login", "Account");
                appUser = await _context.Users.FindAsync(parsedUserId);
            }

            if (appUser == null) return RedirectToAction("Login", "Account");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == appUser.Id);

            var isEnrolled = false;
            if (student != null)
            {
                isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == student.Id && e.CourseId == module.CourseId && (e.Status == "In-Progress" || e.Status == "Completed"));
            }
            if (!isEnrolled) return Forbid();

            // Fetch materials directly to avoid relying on navigation properties that may not be loaded
            List<CourseMaterial> materials = new List<CourseMaterial>();
            try
            {
                var entityType = _context.Model.FindEntityType(typeof(CourseMaterial));
                if (entityType != null)
                {
                    materials = await _context.CourseMaterials
                        .Where(cm => cm.ModuleId == id)
                        .ToListAsync();
                }
                else
                {
                    ViewBag.Warning = "Course materials are not available in this installation.";
                    materials = new List<CourseMaterial>();
                }
            }
            catch (Exception ex)
            {
                // don't throw DB provider exceptions to the end user; show friendly message
                ViewBag.Warning = "Unable to load course materials.";
                materials = new List<CourseMaterial>();
            }

            ViewBag.ModuleName = module.ModuleName;
            ViewBag.ModuleCode = module.ModuleCode;
            ViewBag.CourseName = module.Course?.CourseName ?? string.Empty;

            // Render the student-facing ModuleContent view located at Views/Student/ModuleContent.cshtml
            return View("~/Views/Student/ModuleContent.cshtml", materials);
        }

        public async Task<IActionResult> DownloadMaterial(int id)
        {
            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null || string.IsNullOrEmpty(material.FilePath))
                return NotFound("Material not found.");

            // Resolve absolute vs relative paths. If the stored path is a URL, redirect to it.
            var rawPath = (material.FilePath ?? string.Empty).Trim();

            // If the path appears to be an external URL (starts with http:// or https://), redirect the user to it instead of attempting file IO.
            if (rawPath.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) || rawPath.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(rawPath);
            }

            string resolvedPath;

            // If the path is already absolute on the host, use it directly.
            if (Path.IsPathRooted(rawPath))
            {
                resolvedPath = rawPath;
            }
            else
            {
                // Normalize the relative path: remove leading '~/','/','\\' to avoid Path.Combine issues
                var relative = rawPath.TrimStart('~', '/', '\\');

                // If relative begins with 'wwwroot', strip it so we can combine cleanly with the application's web root.
                if (relative.StartsWith("wwwroot", System.StringComparison.OrdinalIgnoreCase))
                {
                    relative = relative.Substring("wwwroot".Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                }

                var cwd = Directory.GetCurrentDirectory();

                // If the current working directory already points to wwwroot, don't add an extra wwwroot segment.
                if (cwd.EndsWith(Path.DirectorySeparatorChar + "wwwroot") || cwd.EndsWith("wwwroot", System.StringComparison.OrdinalIgnoreCase))
                {
                    resolvedPath = Path.GetFullPath(Path.Combine(cwd, relative));
                }
                else
                {
                    // Combine application root and wwwroot folder to form the full path in a portable way.
                    resolvedPath = Path.GetFullPath(Path.Combine(cwd, "wwwroot", relative));
                }
            }

            if (!System.IO.File.Exists(resolvedPath))
                return NotFound("Physical file not found on server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(resolvedPath);

            // Determine a sensible file name: prefer stored material name + original extension, fallback to filename from path
            var ext = Path.GetExtension(resolvedPath);
            var fileName = !string.IsNullOrEmpty(ext) ? material.MaterialName + ext : Path.GetFileName(resolvedPath);

            // Resolve MIME type using FileExtensionContentTypeProvider for better browser compatibility (handles .docx, .pptx, etc.)
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(resolvedPath, out var contentType))
            {
                // Try basic mapping for common office types if provider misses them
                switch (ext?.ToLowerInvariant())
                {
                    case ".pdf": contentType = "application/pdf"; break;
                    case ".doc": contentType = "application/msword"; break;
                    case ".docx": contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;
                    case ".ppt": contentType = "application/vnd.ms-powerpoint"; break;
                    case ".pptx": contentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation"; break;
                    default: contentType = "application/octet-stream"; break;
                }
            }

            return File(fileBytes, contentType, fileName);
        }
    }
}