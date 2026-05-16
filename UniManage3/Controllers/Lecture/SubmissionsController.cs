using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<SubmissionsController> _logger;

        public SubmissionsController(ApplicationDbContext db, IWebHostEnvironment env, ILogger<SubmissionsController> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
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
            _logger.LogInformation("GradeSubmission POST invoked");
            if (vm == null)
            {
                _logger.LogWarning("GradeSubmission called with null model");
                return BadRequest();
            }

            _logger.LogInformation("Received grading data: Id={Id}, Marks={Marks}, Grade={Grade}, ReviewLength={ReviewLength}",
                vm.Id, vm.Marks, vm.Grade, vm.Review?.Length ?? 0);

            ModelState.Remove(nameof(vm.StudentName));
            ModelState.Remove(nameof(vm.AssignmentName));
            ModelState.Remove(nameof(vm.SubmittedTime));
            ModelState.Remove(nameof(vm.Status));
            ModelState.Remove(nameof(vm.DownloadPath));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid for GradeSubmission");
                foreach (var kv in ModelState)
                {
                    if (kv.Value.Errors != null && kv.Value.Errors.Count > 0)
                    {
                        foreach (var e in kv.Value.Errors)
                        {
                            _logger.LogWarning("ModelState error - {Key}: {Error}", kv.Key, e.ErrorMessage);
                        }
                    }
                }

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
            if (submission == null)
            {
                _logger.LogWarning("No submission found for Id={Id}", vm.Id);
                return NotFound();
            }

            submission.Marks = vm.Marks;
            submission.Grade = vm.Grade;
            submission.Review = vm.Review;

            try
            {
                _db.Update(submission);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Submission {Id} graded: Marks={Marks}, Grade={Grade}", submission.Id, submission.Marks, submission.Grade);
                TempData["SuccessMessage"] = "Submission graded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving grading for submission {Id}", vm.Id);
                TempData["ErrorMessage"] = "Unable to save grading. " + ex.Message;

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
            if (submission == null || string.IsNullOrWhiteSpace(submission.SubmittedFilePath))
            {
                _logger.LogWarning("DownloadSubmission: submission missing or no path for Id={Id}", id);
                return NotFound();
            }

            // Ensure we don't treat a stored path with a leading slash as a rooted path that would ignore web root.
            var relativePath = submission.SubmittedFilePath.TrimStart('~', '/', '\\');
            // Normalize separators to platform specific char
            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            var webRoot = _env.WebRootPath ?? string.Empty;
            var filePath = Path.Combine(webRoot, relativePath);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("DownloadSubmission: file not found. Expected path={Path} (submission id={Id})", filePath, id);
                return NotFound();
            }

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream",
            };

            var fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, contentType, fileName);
        }
    }
}
