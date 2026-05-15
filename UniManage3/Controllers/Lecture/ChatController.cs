using Microsoft.AspNetCore.Mvc;
using UniManage3.Data;
using UniManage3.Models;
using UniManage3.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UniManage3.Controllers.Lecture
{
    [Authorize(Roles = "Lecturer")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ChatController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Index: show student directory for this lecturer's modules
        [HttpGet]
        public IActionResult Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            int userId = int.Parse(userIdClaim);

            // find lecturer record
            var lecturer = _db.Lecturers.FirstOrDefault(l => l.UserId == userId);
            if (lecturer == null) return Forbid();

            // students enrolled in modules where this lecturer is assigned as Module.LecturerId
            var studentIds = _db.Modules
                .Where(m => m.LecturerId == lecturer.Id)
                .SelectMany(m => _db.Enrollments.Where(e => e.CourseId == m.CourseId).Select(e => e.StudentId))
                .Distinct()
                .ToList();

            var students = _db.Students
                .Where(s => studentIds.Contains(s.Id))
                .Select(s => new StudentDirectoryItemViewModel
                {
                    Id = s.Id,
                    FullName = s.FirstName + " " + s.LastName,
                    ContactNumber = s.ContactNumber,
                    NICNumber = s.NICNumber
                })
                .ToList();

            var batches = _db.Batches
                .Select(b => new BatchViewModel { Id = b.Id, BatchName = b.BatchName })
                .ToList();

            var vm = new ChatIndexViewModel
            {
                Students = students,
                Batches = batches
            };

            return View(vm);
        }

        // AJAX: get chat thread for a student
        [HttpGet]
        public IActionResult GetChatThread(int studentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int userId = int.Parse(userIdClaim);
            var lecturer = _db.Lecturers.FirstOrDefault(l => l.UserId == userId);
            if (lecturer == null) return Forbid();

            var thread = _db.CommunicationHubs
                .Where(c => c.StudentId == studentId && c.LecturerId == lecturer.Id)
                .OrderBy(c => c.SentAt)
                .ToList();

            // mark unread messages as read for student view? Keep logic minimal: mark any where AdminReply != null as handled

            var result = thread.Select(c => new
            {
                c.Id,
                c.StudentId,
                c.LecturerId,
                Subject = c.SubjectCategory.ToString(),
                Message = c.MessageContent,
                Reply = c.AdminReply,
                SentAt = c.SentAt.ToString("yyyy-MM-dd HH:mm"),
                RepliedAt = c.RepliedAt?.ToString("yyyy-MM-dd HH:mm")
            });

            return Json(result);
        }

        // AJAX: send message as lecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage([FromForm] int studentId, [FromForm] int subjectCategory, [FromForm] string message)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int userId = int.Parse(userIdClaim);
            var lecturer = _db.Lecturers.FirstOrDefault(l => l.UserId == userId);
            if (lecturer == null) return Forbid();

            // Look for an open communication (MessageContent present but AdminReply null)
            var open = _db.CommunicationHubs.FirstOrDefault(c => c.StudentId == studentId && c.LecturerId == lecturer.Id && string.IsNullOrEmpty(c.AdminReply));

            if (open != null)
            {
                open.AdminReply = message;
                open.RepliedAt = DateTime.UtcNow;
                open.IsReadByStudent = false;
                _db.SaveChanges();
            }
            else
            {
                // Create a new record representing lecturer-initiated message (set MessageContent empty and put reply in AdminReply)
                var comm = new CommunicationHub
                {
                    StudentId = studentId,
                    LecturerId = lecturer.Id,
                    SubjectCategory = (SubjectCategory)subjectCategory,
                    MessageContent = string.Empty,
                    AdminReply = message,
                    SentAt = DateTime.UtcNow,
                    RepliedAt = DateTime.UtcNow,
                    IsReadByStudent = false
                };
                _db.CommunicationHubs.Add(comm);
                _db.SaveChanges();
            }

            return Json(new { success = true, timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") });
        }
    }
}
