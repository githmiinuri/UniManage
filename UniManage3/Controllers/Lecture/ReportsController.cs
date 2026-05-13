using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
using UniManage3.Models.ViewModels;

namespace UniManage3.Controllers.Lecture
{
    [Authorize(Roles = "Lecturer")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReportsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Lecture/Reports
        public async Task<IActionResult> Index(int? moduleId, int? batchId)
        {
            // Identify current user by email or name
            var email = User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Challenge();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return Forbid();

            var lecturer = await _db.Lecturers.FirstOrDefaultAsync(l => l.UserId == user.Id);
            if (lecturer == null) return Forbid();

            // Modules assigned to lecturer
            var modules = await _db.Modules
                .Where(m => m.LecturerId == lecturer.Id)
                .OrderBy(m => m.ModuleName)
                .AsNoTracking()
                .ToListAsync();

            var batches = await _db.Batches.OrderBy(b => b.BatchName).AsNoTracking().ToListAsync();

            var vm = new LecturerReportViewModel
            {
                Modules = new SelectList(modules, "Id", "ModuleName", moduleId),
                Batches = new SelectList(batches, "Id", "BatchName", batchId),
                SelectedModuleId = moduleId,
                SelectedBatchId = batchId
            };

            // If filters provided, query submissions
            if (moduleId.HasValue && batchId.HasValue)
            {
                var query = _db.AssignmentSubmissions
                    .Include(s => s.Student)
                    .Include(s => s.Assignment).ThenInclude(a => a.Module)
                    .Where(s => s.Assignment.ModuleId == moduleId.Value && s.Assignment.BatchId == batchId.Value)
                    .AsQueryable();

                var submissions = await query.OrderByDescending(s => s.SubmittedTime).ToListAsync();

                vm.Submissions = submissions;

                vm.TotalSubmissions = submissions.Count;
                var marksList = submissions.Where(s => s.Marks.HasValue).Select(s => s.Marks.Value).ToList();
                vm.AverageMarks = marksList.Count == 0 ? (double?)null : marksList.Average();
                vm.LateSubmissions = submissions.Count(s => s.Status == SubmissionStatus.Late);
            }

            return View(vm);
        }
    }
}
