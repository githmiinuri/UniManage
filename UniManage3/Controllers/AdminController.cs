using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using UniManage3.Data;
using UniManage3.Models;

namespace UniManage3.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Lecturer actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveLecturer(int id)
        {
            var lect = _db.Lecturers.FirstOrDefault(l => l.Id == id);
            if (lect == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == lect.UserId);
            if (user == null) return NotFound();
            user.IsApproved = true;
            user.IsActive = true;
            _db.SaveChanges();
            return RedirectToAction("LecturerManagement", new { status = "active" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DisapproveLecturer(int id)
        {
            var lect = _db.Lecturers.FirstOrDefault(l => l.Id == id);
            if (lect == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == lect.UserId);
            if (user != null)
            {
                _db.Users.Remove(user);
            }
            // ensure lecturer record removed as well
            _db.Lecturers.Remove(lect);
            _db.SaveChanges();
            return RedirectToAction("LecturerManagement", new { status = "pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuspendLecturer(int id)
        {
            var lect = _db.Lecturers.FirstOrDefault(l => l.Id == id);
            if (lect == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == lect.UserId);
            if (user == null) return NotFound();
            user.IsActive = false;
            // keep IsApproved true
            _db.SaveChanges();
            return RedirectToAction("LecturerManagement", new { status = "suspended" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReinstateLecturer(int id)
        {
            var lect = _db.Lecturers.FirstOrDefault(l => l.Id == id);
            if (lect == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == lect.UserId);
            if (user == null) return NotFound();
            user.IsActive = true;
            _db.SaveChanges();
            return RedirectToAction("LecturerManagement", new { status = "active" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteLecturer(int id)
        {
            var lect = _db.Lecturers.FirstOrDefault(l => l.Id == id);
            if (lect == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == lect.UserId);
            if (user != null)
            {
                _db.Users.Remove(user);
            }
            // always remove lecturer record as well
            _db.Lecturers.Remove(lect);
            _db.SaveChanges();
            return RedirectToAction("LecturerManagement", new { status = "pending" });
        }

        public IActionResult Index()
        {
            // Render the renamed Admin view (Views/Admin/Admin.cshtml)
            return View("Admin");
        }

        // Forwarding action so /Admin/Departments works and opens the Departments view
        public IActionResult Departments(string searchTerm = null)
        {
            return RedirectToAction("Index", "Departments", new { searchTerm });
        }

        // Diagnostic/plain admin page that does not use the shared layout.
        public IActionResult Plain()
        {
            return View("AdminPlain");
        }

        // Student Management page for the User Management submenu.
        // status: "pending", "active", "suspended". searchTerm matches student Id
        public IActionResult StudentManagement(string status = "pending", string searchTerm = null)
        {
            // Note: role/status is not defined on Student; using User.IsActive/IsApproved.
            // Build queries that return joined student+user and materialize before string conversions to avoid expression tree translation issues.

            // parse search term if provided
            int? idFilter = null;
            if (!string.IsNullOrWhiteSpace(searchTerm) && int.TryParse(searchTerm, out var parsed)) idFilter = parsed;

            var pendingQuery = _db.Students.Join(_db.Users, s => s.UserId, u => u.Id, (s, u) => new { s, u })
                                .Where(x => x.u.IsApproved == false);
            var activeQuery = _db.Students.Join(_db.Users, s => s.UserId, u => u.Id, (s, u) => new { s, u })
                                .Where(x => x.u.IsApproved == true && x.u.IsActive == true);
            var suspendedQuery = _db.Students.Join(_db.Users, s => s.UserId, u => u.Id, (s, u) => new { s, u })
                                .Where(x => x.u.IsApproved == true && x.u.IsActive == false);

            if (idFilter.HasValue)
            {
                pendingQuery = pendingQuery.Where(x => x.s.Id == idFilter.Value);
                activeQuery = activeQuery.Where(x => x.s.Id == idFilter.Value);
                suspendedQuery = suspendedQuery.Where(x => x.s.Id == idFilter.Value);
            }

            var pendingList = pendingQuery.OrderByDescending(x => x.s.Id).ToList()
                .Select(x => new StudentItemDto
                {
                    StudentId = x.s.Id,
                    FullName = (x.s.FirstName ?? "") + " " + (x.s.LastName ?? ""),
                    Address = string.Join(", ", new[] { x.s.AddressLine1, x.s.AddressLine2, x.s.City, x.s.Province }.Where(z => !string.IsNullOrWhiteSpace(z))),
                    ContactNumber = x.s.ContactNumber.HasValue ? x.s.ContactNumber.Value.ToString() : string.Empty,
                    NICNumber = x.s.NICNumber,
                    ZipCode = x.s.ZipCode.HasValue ? x.s.ZipCode.Value.ToString() : string.Empty,
                    CreatedAt = x.u.CreatedAt
                }).ToList();

            var activeList = activeQuery.OrderByDescending(x => x.s.Id).ToList()
                .Select(x => new StudentItemDto
                {
                    StudentId = x.s.Id,
                    FullName = (x.s.FirstName ?? "") + " " + (x.s.LastName ?? ""),
                    Address = string.Join(", ", new[] { x.s.AddressLine1, x.s.AddressLine2, x.s.City, x.s.Province }.Where(z => !string.IsNullOrWhiteSpace(z))),
                    ContactNumber = x.s.ContactNumber.HasValue ? x.s.ContactNumber.Value.ToString() : string.Empty,
                    NICNumber = x.s.NICNumber,
                    ZipCode = x.s.ZipCode.HasValue ? x.s.ZipCode.Value.ToString() : string.Empty,
                    CreatedAt = x.u.CreatedAt
                }).ToList();

            var suspendedList = suspendedQuery.OrderByDescending(x => x.s.Id).ToList()
                .Select(x => new StudentItemDto
                {
                    StudentId = x.s.Id,
                    FullName = (x.s.FirstName ?? "") + " " + (x.s.LastName ?? ""),
                    Address = string.Join(", ", new[] { x.s.AddressLine1, x.s.AddressLine2, x.s.City, x.s.Province }.Where(z => !string.IsNullOrWhiteSpace(z))),
                    ContactNumber = x.s.ContactNumber.HasValue ? x.s.ContactNumber.Value.ToString() : string.Empty,
                    NICNumber = x.s.NICNumber,
                    ZipCode = x.s.ZipCode.HasValue ? x.s.ZipCode.Value.ToString() : string.Empty,
                    CreatedAt = x.u.CreatedAt
                }).ToList();

            var vm = new StudentManagementViewModel
            {
                SelectedTab = status?.ToLower() ?? "pending",
                PendingStudents = pendingList,
                ActiveStudents = activeList,
                SuspendedStudents = suspendedList,
                SearchTerm = searchTerm
            };

            return View("StudentManagement", vm);
        }

        // Lecturer management page for the User Management submenu.
        // Lecturer management page for the User Management submenu.
        // status: "pending", "active", "suspended". searchTerm matches lecturer Id
        public IActionResult LecturerManagement(string status = "pending", string searchTerm = null)
        {
            int? idFilter = null;
            if (!string.IsNullOrWhiteSpace(searchTerm) && int.TryParse(searchTerm, out var parsed)) idFilter = parsed;

            var pendingQuery = _db.Lecturers.Join(_db.Users, l => l.UserId, u => u.Id, (l, u) => new { l, u })
                                .Where(x => x.u.IsApproved == false);
            var activeQuery = _db.Lecturers.Join(_db.Users, l => l.UserId, u => u.Id, (l, u) => new { l, u })
                                .Where(x => x.u.IsApproved == true && x.u.IsActive == true);
            var suspendedQuery = _db.Lecturers.Join(_db.Users, l => l.UserId, u => u.Id, (l, u) => new { l, u })
                                .Where(x => x.u.IsApproved == true && x.u.IsActive == false);

            if (idFilter.HasValue)
            {
                pendingQuery = pendingQuery.Where(x => x.l.Id == idFilter.Value);
                activeQuery = activeQuery.Where(x => x.l.Id == idFilter.Value);
                suspendedQuery = suspendedQuery.Where(x => x.l.Id == idFilter.Value);
            }

            var pendingList = pendingQuery.OrderByDescending(x => x.l.Id).ToList()
                .Select(x => new LecturerItemDto
                {
                    LecturerId = x.l.Id,
                    FullName = (x.l.FirstName ?? "") + " " + (x.l.LastName ?? ""),
                    Address = string.Join(", ", new[] { x.l.AddressLine1, x.l.AddressLine2, x.l.City, x.l.Province }.Where(z => !string.IsNullOrWhiteSpace(z))),
                    ContactNumber = x.l.ContactNumber.HasValue ? x.l.ContactNumber.Value.ToString() : string.Empty,
                    NICNumber = x.l.NICNumber,
                    ZipCode = x.l.ZipCode.HasValue ? x.l.ZipCode.Value.ToString() : string.Empty,
                    CreatedAt = x.u.CreatedAt
                }).ToList();

            var activeList = activeQuery.OrderByDescending(x => x.l.Id).ToList()
                .Select(x => new LecturerItemDto
                {
                    LecturerId = x.l.Id,
                    FullName = (x.l.FirstName ?? "") + " " + (x.l.LastName ?? ""),
                    Address = string.Join(", ", new[] { x.l.AddressLine1, x.l.AddressLine2, x.l.City, x.l.Province }.Where(z => !string.IsNullOrWhiteSpace(z))),
                    ContactNumber = x.l.ContactNumber.HasValue ? x.l.ContactNumber.Value.ToString() : string.Empty,
                    NICNumber = x.l.NICNumber,
                    ZipCode = x.l.ZipCode.HasValue ? x.l.ZipCode.Value.ToString() : string.Empty,
                    CreatedAt = x.u.CreatedAt
                }).ToList();

            var suspendedList = suspendedQuery.OrderByDescending(x => x.l.Id).ToList()
                .Select(x => new LecturerItemDto
                {
                    LecturerId = x.l.Id,
                    FullName = (x.l.FirstName ?? "") + " " + (x.l.LastName ?? ""),
                    Address = string.Join(", ", new[] { x.l.AddressLine1, x.l.AddressLine2, x.l.City, x.l.Province }.Where(z => !string.IsNullOrWhiteSpace(z))),
                    ContactNumber = x.l.ContactNumber.HasValue ? x.l.ContactNumber.Value.ToString() : string.Empty,
                    NICNumber = x.l.NICNumber,
                    ZipCode = x.l.ZipCode.HasValue ? x.l.ZipCode.Value.ToString() : string.Empty,
                    CreatedAt = x.u.CreatedAt
                }).ToList();

            var vm = new LecturerManagementViewModel
            {
                SelectedTab = status?.ToLower() ?? "pending",
                PendingLecturers = pendingList,
                ActiveLecturers = activeList,
                SuspendedLecturers = suspendedList,
                SearchTerm = searchTerm
            };

            return View("LecturerManagement", vm);
        }

        // Administrator management page for the User Management submenu.
        // status: "pending", "active", "suspended". searchTerm matches administrator Id
        public IActionResult AdministratorManagement(string status = "pending", string searchTerm = null)
        {
            // Use the same EF patterns as StudentManagement but via Include to ensure navigation is loaded
            int? idFilter = null;
            if (!string.IsNullOrWhiteSpace(searchTerm) && int.TryParse(searchTerm, out var parsed)) idFilter = parsed;

            var adminsWithUsers = _db.Administrators.Include(a => a.User).AsQueryable();

            var pendingQuery = adminsWithUsers.Where(x => x.User != null && x.User.IsApproved == false);
            var activeQuery = adminsWithUsers.Where(x => x.User != null && x.User.IsApproved == true && x.User.IsActive == true);
            var suspendedQuery = adminsWithUsers.Where(x => x.User != null && x.User.IsApproved == true && x.User.IsActive == false);

            if (idFilter.HasValue)
            {
                pendingQuery = pendingQuery.Where(x => x.Id == idFilter.Value);
                activeQuery = activeQuery.Where(x => x.Id == idFilter.Value);
                suspendedQuery = suspendedQuery.Where(x => x.Id == idFilter.Value);
            }

            var pendingList = pendingQuery.OrderByDescending(x => x.Id).ToList()
                .Select(x => new AdminItemDto
                {
                    AdminId = x.Id,
                    FullName = (x.FirstName ?? "") + " " + (x.LastName ?? ""),
                    Address = string.Join(", ", new[] { x.AddressLine1, x.AddressLine2, x.City, x.Province }.Where(z => !string.IsNullOrWhiteSpace(z))),
                    ContactNumber = x.ContactNumber.HasValue ? x.ContactNumber.Value.ToString() : string.Empty,
                    NICNumber = x.NICNumber,
                    ZipCode = x.ZipCode.HasValue ? x.ZipCode.Value.ToString() : string.Empty,
                    CreatedAt = x.User?.CreatedAt ?? DateTime.MinValue
                }).ToList();

            var activeList = activeQuery.OrderByDescending(x => x.Id).ToList()
                .Select(x => new AdminItemDto
                {
                    AdminId = x.Id,
                    FullName = (x.FirstName ?? "") + " " + (x.LastName ?? ""),
                    Address = string.Join(", ", new[] { x.AddressLine1, x.AddressLine2, x.City, x.Province }.Where(z => !string.IsNullOrWhiteSpace(z))),
                    ContactNumber = x.ContactNumber.HasValue ? x.ContactNumber.Value.ToString() : string.Empty,
                    NICNumber = x.NICNumber,
                    ZipCode = x.ZipCode.HasValue ? x.ZipCode.Value.ToString() : string.Empty,
                    CreatedAt = x.User?.CreatedAt ?? DateTime.MinValue
                }).ToList();

            var suspendedList = suspendedQuery.OrderByDescending(x => x.Id).ToList()
                .Select(x => new AdminItemDto
                {
                    AdminId = x.Id,
                    FullName = (x.FirstName ?? "") + " " + (x.LastName ?? ""),
                    Address = string.Join(", ", new[] { x.AddressLine1, x.AddressLine2, x.City, x.Province }.Where(z => !string.IsNullOrWhiteSpace(z))),
                    ContactNumber = x.ContactNumber.HasValue ? x.ContactNumber.Value.ToString() : string.Empty,
                    NICNumber = x.NICNumber,
                    ZipCode = x.ZipCode.HasValue ? x.ZipCode.Value.ToString() : string.Empty,
                    CreatedAt = x.User?.CreatedAt ?? DateTime.MinValue
                }).ToList();

            var vm = new AdminManagementViewModel
            {
                SelectedTab = status?.ToLower() ?? "pending",
                PendingAdmins = pendingList,
                ActiveAdmins = activeList,
                SuspendedAdmins = suspendedList,
                SearchTerm = searchTerm,
                PendingCount = pendingList.Count,
                ActiveCount = activeList.Count,
                SuspendedCount = suspendedList.Count
            };

            return View("AdminManagement", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveAdministrator(int id)
        {
            var adm = _db.Administrators.FirstOrDefault(a => a.Id == id);
            if (adm == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == adm.UserId);
            if (user == null) return NotFound();
            user.IsApproved = true;
            user.IsActive = true;
            _db.SaveChanges();
            return RedirectToAction("AdministratorManagement", new { status = "active" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DisapproveAdministrator(int id)
        {
            var adm = _db.Administrators.FirstOrDefault(a => a.Id == id);
            if (adm == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == adm.UserId);
            if (user != null)
            {
                _db.Users.Remove(user);
            }
            // ensure administrator record removed as well
            _db.Administrators.Remove(adm);
            _db.SaveChanges();
            return RedirectToAction("AdministratorManagement", new { status = "pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuspendAdministrator(int id)
        {
            var adm = _db.Administrators.FirstOrDefault(a => a.Id == id);
            if (adm == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == adm.UserId);
            if (user == null) return NotFound();
            user.IsActive = false;
            _db.SaveChanges();
            return RedirectToAction("AdministratorManagement", new { status = "suspended" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReinstateAdministrator(int id)
        {
            var adm = _db.Administrators.FirstOrDefault(a => a.Id == id);
            if (adm == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == adm.UserId);
            if (user == null) return NotFound();
            user.IsActive = true;
            _db.SaveChanges();
            return RedirectToAction("AdministratorManagement", new { status = "active" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAdministrator(int id)
        {
            var adm = _db.Administrators.FirstOrDefault(a => a.Id == id);
            if (adm == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == adm.UserId);
            if (user != null)
            {
                _db.Users.Remove(user);
            }
            _db.Administrators.Remove(adm);
            _db.SaveChanges();
            return RedirectToAction("AdministratorManagement", new { status = "pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveStudent(int id)
        {
            var student = _db.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == student.UserId);
            if (user == null) return NotFound();
            user.IsApproved = true;
            user.IsActive = true;
            _db.SaveChanges();
            return RedirectToAction("StudentManagement", new { status = "active" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DisapproveStudent(int id)
        {
            var student = _db.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == student.UserId);
            if (user != null)
            {
                _db.Users.Remove(user);
            }
            else
            {
                _db.Students.Remove(student);
            }
            _db.SaveChanges();
            return RedirectToAction("StudentManagement", new { status = "pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuspendStudent(int id)
        {
            var student = _db.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == student.UserId);
            if (user == null) return NotFound();
            user.IsActive = false;
            // keep IsApproved true
            _db.SaveChanges();
            return RedirectToAction("StudentManagement", new { status = "suspended" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReinstateStudent(int id)
        {
            var student = _db.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == student.UserId);
            if (user == null) return NotFound();
            user.IsActive = true;
            _db.SaveChanges();
            return RedirectToAction("StudentManagement", new { status = "active" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStudent(int id)
        {
            var student = _db.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            var user = _db.Users.FirstOrDefault(u => u.Id == student.UserId);
            if (user != null)
            {
                _db.Users.Remove(user);
            }
            else
            {
                _db.Students.Remove(student);
            }
            _db.SaveChanges();
            return RedirectToAction("StudentManagement", new { status = "pending" });
        }
    }
}
