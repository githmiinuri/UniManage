using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
using UniManage3.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UniManage3.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Main action for the Enrollment Management page.
        /// Handles filtering by status and searching.
        /// </summary>
        public async Task<IActionResult> Enrollments(string status = "Pending", string searchTerm = "")
        {
            // Default to Pending (In-Progress) if status is null or empty
            if (string.IsNullOrEmpty(status)) status = "Pending";

            // Map the UI "Pending" tab to the database "In-Progress" status
            string dbStatus = status;
            if (status == "Pending") dbStatus = "In-Progress";

            // 1. Fetch relevant enrollments based on status
            // We use a projection to avoid issues with nullable types in the DB provider
            var enrollmentsQuery = _context.Enrollments.AsQueryable();

            // Note: Per requirements, we focus on specific tabs. 
            // If status is not "All", filter by the specific dbStatus.
            if (status != "All")
            {
                enrollmentsQuery = enrollmentsQuery.Where(e => e.Status == dbStatus);
            }

            var enrollmentsData = await enrollmentsQuery
                .Select(e => new {
                    e.Id,
                    e.StudentId,
                    e.CourseId,
                    e.EnrollmentDate,
                    e.Status
                })
                .ToListAsync();

            // 2. Load necessary related data into dictionaries for mapping
            var studentIds = enrollmentsData.Select(e => e.StudentId).Distinct().ToList();
            var studentMap = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .Select(s => new {
                    s.Id,
                    FullName = s.User != null ? s.User.FullName : "Unknown",
                    Email = s.User != null ? s.User.Email : "N/A"
                })
                .ToDictionaryAsync(s => s.Id, s => new { s.FullName, s.Email });

            var courseIds = enrollmentsData.Select(e => e.CourseId).Distinct().ToList();
            var courses = await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .Select(c => new {
                    c.Id,
                    c.CourseCode,
                    c.CourseName,
                    c.PrerequisiteCourseId // The ID of the required course
                })
                .ToListAsync();

            // To map PrerequisiteCourseId to a Name, we need to fetch names for ALL referenced prerequisites
            var allPrereqIds = courses.Where(c => c.PrerequisiteCourseId.HasValue)
                                      .Select(c => c.PrerequisiteCourseId.Value)
                                      .Distinct().ToList();

            var prereqNameMap = await _context.Courses
                .Where(c => allPrereqIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CourseName);

            var courseMap = courses.ToDictionary(c => c.Id);

            // 3. Project to the ViewModel
            var projected = new List<EnrollmentItemViewModel>();
            foreach (var e in enrollmentsData)
            {
                var st = studentMap.ContainsKey(e.StudentId) ? studentMap[e.StudentId] : null;
                var ct = courseMap.ContainsKey(e.CourseId) ? courseMap[e.CourseId] : null;

                string prereqName = "None";
                if (ct?.PrerequisiteCourseId != null && prereqNameMap.TryGetValue(ct.PrerequisiteCourseId.Value, out var pName))
                {
                    prereqName = pName;
                }

                projected.Add(new EnrollmentItemViewModel
                {
                    Id = e.Id,
                    StudentName = st?.FullName ?? "Unknown",
                    StudentEmail = st?.Email ?? "N/A",
                    CourseCode = ct?.CourseCode ?? "N/A",
                    CourseName = ct?.CourseName ?? "N/A",
                    EnrollmentDate = e.EnrollmentDate,
                    PrerequisiteCourseId = ct?.PrerequisiteCourseId,
                    PrerequisiteName = prereqName, // Now holds the actual Name of the prerequisite course
                    PrerequisiteMet = true // Business logic for eligibility check could go here
                });
            }

            // 4. In-Memory Search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                projected = projected.Where(p =>
                    p.StudentName.ToLower().Contains(term) ||
                    p.CourseName.ToLower().Contains(term) ||
                    p.CourseCode.ToLower().Contains(term)
                ).ToList();
            }

            // 5. Prepare final ViewModel with counts for the status cards
            var viewModel = new EnrollmentManagementViewModel
            {
                Enrollments = projected,
                CurrentStatus = status,
                SearchTerm = searchTerm,
                PendingCount = await _context.Enrollments.CountAsync(e => e.Status == "In-Progress"),
                CompletedCount = await _context.Enrollments.CountAsync(e => e.Status == "Completed"),
                FailedCount = await _context.Enrollments.CountAsync(e => e.Status == "Failed")
            };

            return View(viewModel);
        }

        /// <summary>
        /// Approves a pending enrollment request.
        /// Only allowed for In-Progress records.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return Json(new { success = false, message = "Record not found." });

            if (enrollment.Status != "In-Progress")
                return Json(new { success = false, message = "Only pending enrollments can be approved." });

            enrollment.Status = "Completed";
            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Enrollment approved.",
                pendingCount = await _context.Enrollments.CountAsync(e => e.Status == "In-Progress"),
                completedCount = await _context.Enrollments.CountAsync(e => e.Status == "Completed"),
                failedCount = await _context.Enrollments.CountAsync(e => e.Status == "Failed")
            });
        }

        /// <summary>
        /// Rejects a pending enrollment request.
        /// Only allowed for In-Progress records.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return Json(new { success = false, message = "Record not found." });

            if (enrollment.Status != "In-Progress")
                return Json(new { success = false, message = "Only pending enrollments can be rejected." });

            enrollment.Status = "Failed";
            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Enrollment rejected.",
                pendingCount = await _context.Enrollments.CountAsync(e => e.Status == "In-Progress"),
                completedCount = await _context.Enrollments.CountAsync(e => e.Status == "Completed"),
                failedCount = await _context.Enrollments.CountAsync(e => e.Status == "Failed")
            });
        }

        /// <summary>
        /// Permanently deletes an enrollment record.
        /// Designed for cleaning up Completed or Failed records.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return Json(new { success = false, message = "Record not found." });

            // Only allow permanent deletion for non-pending records
            if (enrollment.Status == "In-Progress")
            {
                return Json(new { success = false, message = "Cannot delete a pending enrollment." });
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Record permanently deleted.",
                pendingCount = await _context.Enrollments.CountAsync(e => e.Status == "In-Progress"),
                completedCount = await _context.Enrollments.CountAsync(e => e.Status == "Completed"),
                failedCount = await _context.Enrollments.CountAsync(e => e.Status == "Failed")
            });
        }
    }
}