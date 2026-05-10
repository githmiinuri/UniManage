using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UniManage3.Data;
using UniManage3.Models;
using UniManage3.Models.ViewModels;

namespace UniManage3.Controllers.Lecture
{
    [Authorize(Roles = "Lecturer")]
    public class LectureDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LectureDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LectureDashboard
        public async Task<IActionResult> Index()
        {
            var vm = new LectureDashboardViewModel();

            try
            {
                // Identify current user by common claims
                var email = User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return Challenge();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    vm.ErrorMessage = "Unable to locate your user profile.";
                    return View(vm);
                }

                var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user.Id);
                if (lecturer == null)
                {
                    vm.ErrorMessage = "No lecturer profile found for the current user.";
                    return View(vm);
                }

                vm.LecturerName = string.IsNullOrWhiteSpace(lecturer.FirstName) && string.IsNullOrWhiteSpace(lecturer.LastName)
                    ? user.FullName
                    : (lecturer.FirstName + " " + lecturer.LastName).Trim();

                // Fetch modules assigned to this lecturer. Include Course and Assignments.
                var modules = await _context.Modules
                    .Where(m => m.LecturerId == lecturer.Id)
                    .Include(m => m.Course)
                    .Include(m => m.Assignments)
                    .AsNoTracking()
                    .ToListAsync();

                // Protect against null
                modules = modules ?? new System.Collections.Generic.List<Module>();

                vm.AssignedModules = modules;

                // Derive distinct courses
                var courses = modules
                    .Where(m => m.Course != null)
                    .Select(m => m.Course)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .ToList();

                vm.AssignedCourses = courses;

                // KPIs
                vm.TotalModulesAssigned = modules.Count;

                // Count ongoing assignments: deadline >= now
                var now = DateTime.Now;
                vm.OngoingAssignments = modules
                    .Where(m => m.Assignments != null)
                    .SelectMany(m => m.Assignments)
                    .Count(a => a.DeadlineDate >= now);

                // Dummy total enrolled students: random between 50 and 200 scaled by number of courses (simple heuristic)
                var rnd = new Random();
                var baseValue = rnd.Next(50, 201);
                vm.TotalEnrolledStudents = Math.Max(50, baseValue + (courses.Count * 5));

                // --- Fetch latest 3 real submissions from DB related to this lecturer ---
                var latestAnonymous = await _context.AssignmentSubmissions
                    .AsNoTracking()
                    .Include(s => s.Student)
                    .Include(s => s.Assignment).ThenInclude(a => a.Module)
                    .Where(s => s.Assignment != null && s.Assignment.Module != null && s.Assignment.Module.LecturerId == lecturer.Id)
                    .OrderByDescending(s => s.SubmittedTime)
                    .Take(3)
                    .Select(s => new
                    {
                        s.Id,
                        StudentName = s.Student != null ? s.Student.FullName : null,
                        AssignmentName = s.Assignment != null ? s.Assignment.AssignmentName : null,
                        ModuleName = s.Assignment.Module.ModuleName,
                        s.SubmittedTime,
                        StatusInt = (int)s.Status
                    })
                    .ToListAsync();

                var latestSubmissions = latestAnonymous
                    .Select(s => new LectureDashboardViewModel.DummyStudentSubmission
                    {
                        SubmissionId = s.Id,
                        StudentName = s.StudentName ?? "Unknown",
                        AssignmentName = s.AssignmentName ?? "Unknown",
                        ModuleName = s.ModuleName ?? "Unknown",
                        SubmittedDate = s.SubmittedTime,
                        Status = Enum.GetName(typeof(SubmissionStatus), s.StatusInt) ?? s.StatusInt.ToString()
                    })
                    .ToList();

                vm.RecentSubmissions = latestSubmissions;

                // --- Real assignment-wise grading progress (two-step: server-side aggregate, client-side mapping/sort) ---
                var grouped = await _context.AssignmentSubmissions
                    .AsNoTracking()
                    .Include(s => s.Assignment).ThenInclude(a => a.Module)
                    .Where(s => s.Assignment != null && s.Assignment.Module != null && s.Assignment.Module.LecturerId == lecturer.Id)
                    .GroupBy(s => new
                    {
                        s.AssignmentId,
                        AssignmentName = s.Assignment.AssignmentName,
                        ModuleCode = s.Assignment.Module.ModuleCode
                    })
                    .Select(g => new
                    {
                        g.Key.AssignmentId,
                        g.Key.AssignmentName,
                        g.Key.ModuleCode,
                        TotalSubmissions = g.Count(),
                        GradedCount = g.Count(s => s.Marks != null)
                    })
                    .ToListAsync(); // execute on DB

                var gradingSummaries = grouped
                    .Select(x => new LectureDashboardViewModel.AssignmentGradingProgress
                    {
                        AssignmentName = x.AssignmentName ?? "Unknown",
                        ModuleCode = x.ModuleCode ?? "-",
                        TotalSubmissions = x.TotalSubmissions,
                        GradedCount = x.GradedCount
                    })
                    .OrderByDescending(x => x.PendingCount) // in-memory
                    .Take(5)
                    .ToList();

                vm.GradingSummaries = gradingSummaries;
            }
            catch (Exception ex)
            {
                vm.ErrorMessage = "An error occurred while loading the dashboard." + " " + ex.Message;
            }

            return View(vm);
        }
    }
}
