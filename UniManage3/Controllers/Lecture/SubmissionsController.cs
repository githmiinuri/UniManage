using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
using UniManage3.Models.ViewModels;

namespace UniManage3.Controllers.Lecture
{
    [Authorize(Roles = "Lecturer")]
    public class SubmissionsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public SubmissionsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: Lecture/Submissions
        public async Task<IActionResult> Index(int? batchId)
        {
            var query = _db.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment).ThenInclude(a => a.Batch)
                .AsQueryable();

            if (batchId.HasValue)
            {
                query = query.Where(s => s.Assignment.BatchId == batchId.Value);
            }

            var submissions = await query.OrderByDescending(s => s.SubmittedTime).ToListAsync();

            var batches = await _db.Batches.OrderBy(b => b.BatchName).ToListAsync();

            var vm = new SubmissionListViewModel
            {
                Submissions = submissions,
                Batches = new SelectList(batches, "Id", "BatchName", batchId),
                SelectedBatchId = batchId
            };

            return View(vm);
        }

        // GET: Lecture/Submissions/GradeSubmission/5
        public async Task<IActionResult> GradeSubmission(int id)
        {
            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound();

            var vm = new GradingViewModel
            {
                Id = submission.Id,
                StudentName = submission.Student?.FullName ?? "Unknown",
                AssignmentName = submission.Assignment?.AssignmentName ?? "Unknown",
                SubmittedTime = submission.SubmittedTime,
                Status = submission.Status,
                DownloadPath = submission.SubmittedFilePath,
                Marks = submission.Marks,
                Grade = submission.Grade,
                Review = submission.Review
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(GradingViewModel vm)
        {
            if (vm == null) return BadRequest();

            // reload for view if validation fails
            if (!ModelState.IsValid)
            {
                var existing = await _db.AssignmentSubmissions
                    .Include(s => s.Student)
                    .Include(s => s.Assignment)
                    .FirstOrDefaultAsync(s => s.Id == vm.Id);

                if (existing == null) return NotFound();

                vm.StudentName = existing.Student?.FullName ?? "Unknown";
                vm.AssignmentName = existing.Assignment?.AssignmentName ?? "Unknown";
                vm.SubmittedTime = existing.SubmittedTime;
                vm.Status = existing.Status;
                vm.DownloadPath = existing.SubmittedFilePath;

                return View(vm);
            }

            var submission = await _db.AssignmentSubmissions.FindAsync(vm.Id);
            if (submission == null) return NotFound();

            // Update fields explicitly
            submission.Marks = vm.Marks;
            submission.Grade = vm.Grade;
            submission.Review = vm.Review;

            try
            {
                _db.Update(submission);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Submission graded successfully.";
            }
            catch (Exception ex)
            {
                // Log or handle exception as needed
                TempData["ErrorMessage"] = "Unable to save grading. " + ex.Message;

                // repopulate and return view so user can retry
                var existing = await _db.AssignmentSubmissions
                    .Include(s => s.Student)
                    .Include(s => s.Assignment)
                    .FirstOrDefaultAsync(s => s.Id == vm.Id);
                if (existing == null) return NotFound();

                vm.StudentName = existing.Student?.FullName ?? "Unknown";
                vm.AssignmentName = existing.Assignment?.AssignmentName ?? "Unknown";
                vm.SubmittedTime = existing.SubmittedTime;
                vm.Status = existing.Status;
                vm.DownloadPath = existing.SubmittedFilePath;

                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Lecture/Submissions/DownloadSubmission/5
        public async Task<IActionResult> DownloadSubmission(int id)
        {
            var submission = await _db.AssignmentSubmissions.FindAsync(id);
            if (submission == null || string.IsNullOrEmpty(submission.SubmittedFilePath)) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath ?? string.Empty, submission.SubmittedFilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var contentType = "application/octet-stream";
            // simple content type guessing by extension
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".pdf") contentType = "application/pdf";
            else if (ext == ".doc" || ext == ".docx") contentType = "application/msword";

            var fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, contentType, fileName);
        }
    }
}
