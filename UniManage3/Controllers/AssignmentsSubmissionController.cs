using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using System.IO;
using System.Linq;

namespace UniManage3.Controllers
{
    [Authorize(Roles = "Student")]
    public class AssignmentsSubmissionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AssignmentsSubmissionController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Resolve the application User record from the current ClaimsPrincipal
        private async Task<User?> ResolveAppUserAsync()
        {
            string? email = User?.Identity?.Name;
            User? appUser = null;

            if (!string.IsNullOrEmpty(email))
            {
                appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }

            if (appUser == null)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return null;
                if (int.TryParse(userIdClaim, out var parsedUserId))
                {
                    appUser = await _context.Users.FindAsync(parsedUserId);
                }
                else
                {
                    appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userIdClaim);
                }
            }

            return appUser;
        }

        // Helper to resolve the current Student reliably
        private async Task<Student?> ResolveCurrentStudentAsync(User appUser)
        {
            if (appUser == null) return null;

            // Try find by direct user id
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == appUser.Id);
            if (student != null) return student;

            // Try find by email via related User entity
            if (!string.IsNullOrEmpty(appUser.Email))
            {
                student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.User != null && s.User.Email == appUser.Email);
                if (student != null) return student;
            }

            // Not found — return null to let caller handle redirection/unauthorized
            return null;
        }

        // GET: /AssignmentsSubmission
        public async Task<IActionResult> Index()
        {
            // Resolve current app user and student
            var appUser = await ResolveAppUserAsync();
            if (appUser == null) return RedirectToAction("Login", "Account");


            var student = await ResolveCurrentStudentAsync(appUser);
            if (student == null) return RedirectToAction("MyCourses", "Student");

            // If student has no Batch assigned, return empty list
            if (student.BatchId == null)
            {
                ViewData["AssignmentsWithSubmissions"] = new List<object>();
                return View("~/Views/Student/Assignments.cshtml");
            }

            // Fetch enrolled course ids for the student to ensure assignments are only for courses the student is actually enrolled in
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && (e.Status == "In-Progress" || e.Status == "Completed"))
                .Select(e => e.CourseId)
                .Distinct()
                .ToListAsync();

            // Fetch assignments that belong to the student's batch AND whose module's course is one the student is enrolled in
            var assignments = await _context.Assignments
                .Include(a => a.Module)
                .Include(a => a.Batch)
                .Where(a => a.BatchId == student.BatchId && enrolledCourseIds.Contains(a.Module.CourseId))
                .OrderByDescending(a => a.DeadlineDate)
                .ToListAsync();

            // Fetch submissions for current student only
            var submissionList = await _context.AssignmentSubmissions
                .Where(s => s.StudentId == student.Id && assignments.Select(a => a.Id).Contains(s.AssignmentId))
                .ToListAsync();

            // Left join assignments with the student's submissions so submissions from other students are never included
            var vm = assignments.GroupJoin(submissionList,
                    a => a.Id,
                    s => s.AssignmentId,
                    (a, subs) => new
                    {
                        Assignment = a,
                        Submission = subs.FirstOrDefault()
                    })
                .ToList();

            ViewData["AssignmentsWithSubmissions"] = vm;
            return View("~/Views/Student/Assignments.cshtml");
        }

        // Download brief or assignment resource
        public async Task<IActionResult> DownloadBrief(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null || string.IsNullOrEmpty(assignment.ResourceFilePath)) return NotFound();

            var rawPath = assignment.ResourceFilePath.Trim();
            if (rawPath.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) || rawPath.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(rawPath);
            }

            // Normalize and resolve path relative to web root. Strip leading wwwroot if present in DB value.
            var relative = rawPath.TrimStart('~', '/', '\\').Replace('/', Path.DirectorySeparatorChar);
            if (relative.StartsWith("wwwroot", System.StringComparison.OrdinalIgnoreCase))
            {
                relative = relative.Substring("wwwroot".Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            var webRoot = _env.WebRootPath ?? Directory.GetCurrentDirectory();
            var full = Path.GetFullPath(Path.Combine(webRoot, relative));
            if (!System.IO.File.Exists(full)) return NotFound();

            var contentType = GetContentType(full);
            var fileName = Path.GetFileName(full);
            return PhysicalFile(full, contentType, fileName);
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream",
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignment(int assignmentId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            var appUser = await ResolveAppUserAsync();
            if (appUser == null) return RedirectToAction("Login", "Account");

            var student = await ResolveCurrentStudentAsync(appUser);
            if (student == null) return RedirectToAction("Index", "MyCourses");

            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return NotFound();

            // Ensure assignment belongs to student's batch
            if (student.BatchId != null && assignment.BatchId != student.BatchId)
            {
                return Forbid();
            }

            // determine upload folder
            var uploadsRoot = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), "uploads", "submissions");
            if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{student.Id}_{assignmentId}_{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/submissions/{fileName}";

            // find existing submission
            var existing = await _context.AssignmentSubmissions.FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == student.Id);
            if (existing == null)
            {
                existing = new AssignmentSubmission
                {
                    AssignmentId = assignmentId,
                    StudentId = student.Id,
                    SubmittedFilePath = relativePath,
                    SubmittedTime = DateTime.Now,
                    Status = DateTime.Now <= assignment.DeadlineDate ? SubmissionStatus.OnTime : SubmissionStatus.Late
                };
                _context.AssignmentSubmissions.Add(existing);
            }
            else
            {
                existing.SubmittedFilePath = relativePath;
                existing.SubmittedTime = DateTime.Now;
                existing.Status = DateTime.Now <= assignment.DeadlineDate ? SubmissionStatus.OnTime : SubmissionStatus.Late;
                _context.AssignmentSubmissions.Update(existing);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // AJAX upload endpoint with progress support
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignmentAjax(int assignmentId)
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0) return Json(new { success = false, message = "No file uploaded." });

            string? email = User?.Identity?.Name;
            User? appUser = null;

            if (!string.IsNullOrEmpty(email))
            {
                appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }

            if (appUser == null)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return Json(new { success = false, message = "Not authenticated." });
                if (int.TryParse(userIdClaim, out var parsedUserId))
                {
                    appUser = await _context.Users.FindAsync(parsedUserId);
                }
                else
                {
                    appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userIdClaim);
                }
            }

            if (appUser == null) return Json(new { success = false, message = "Not authenticated." });

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == appUser.Id);
            if (student == null) return Json(new { success = false, message = "Student record not found." });

            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return Json(new { success = false, message = "Assignment not found." });

            // Ensure assignment belongs to student's batch
            if (student.BatchId != null && assignment.BatchId != student.BatchId)
            {
                return Json(new { success = false, message = "Assignment not available for your batch." });
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), "uploads", "submissions");
            if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{student.Id}_{assignmentId}_{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/submissions/{fileName}";

            var existing = await _context.AssignmentSubmissions.FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == student.Id);
            if (existing == null)
            {
                existing = new AssignmentSubmission
                {
                    AssignmentId = assignmentId,
                    StudentId = student.Id,
                    SubmittedFilePath = relativePath,
                    SubmittedTime = DateTime.Now,
                    Status = DateTime.Now <= assignment.DeadlineDate ? SubmissionStatus.OnTime : SubmissionStatus.Late
                };
                _context.AssignmentSubmissions.Add(existing);
            }
            else
            {
                existing.SubmittedFilePath = relativePath;
                existing.SubmittedTime = DateTime.Now;
                existing.Status = DateTime.Now <= assignment.DeadlineDate ? SubmissionStatus.OnTime : SubmissionStatus.Late;
                _context.AssignmentSubmissions.Update(existing);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Uploaded", status = existing.Status.ToString(), submittedTime = existing.SubmittedTime });
        }

        // Assignment detail page
        public async Task<IActionResult> Details(int id)
        {
            var assignment = await _context.Assignments.Include(a => a.Module).Include(a => a.Batch).FirstOrDefaultAsync(a => a.Id == id);
            if (assignment == null) return RedirectToAction("Index");

            // Resolve student
            string? email = User?.Identity?.Name;
            User? appUser = null;
            if (!string.IsNullOrEmpty(email)) appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (appUser == null)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId)) appUser = await _context.Users.FindAsync(parsedUserId);
            }

            var submission = (AssignmentSubmission?)null;
            if (appUser != null)
            {
                var student = await ResolveCurrentStudentAsync(appUser);
                if (student != null)
                {
                    // verify assignment belongs to student's batch before returning submission
                    if (student.BatchId == null || assignment.BatchId != student.BatchId)
                    {
                        return RedirectToAction("Index");
                    }

                    submission = await _context.AssignmentSubmissions.FirstOrDefaultAsync(s => s.AssignmentId == id && s.StudentId == student.Id);
                }
            }

            ViewData["Assignment"] = assignment;
            ViewData["Submission"] = submission;
            return View("~/Views/Student/AssignmentDetails.cshtml");
        }
    }
}