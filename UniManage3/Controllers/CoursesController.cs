using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
using System.Diagnostics;

namespace UniManage3.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CoursesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var courses = await _db.Courses
                .Include(c => c.PrerequisiteCourse)
                .Include(c => c.Department)
                .Include(c => c.Coordinator).ThenInclude(l => l.User)
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            // The project uses a custom view name `Courses.cshtml` (not the default Index.cshtml).
            // Return the view explicitly by name so the sidebar link to /Courses/Index loads that file.
            return View("Courses", courses);
        }

        // GET: Courses/Row/5 - returns a partial table row for AJAX refresh
        [HttpGet]
        public async Task<IActionResult> Row(int id)
        {
            var c = await _db.Courses
                .Include(x => x.PrerequisiteCourse)
                .Include(x => x.Coordinator).ThenInclude(l => l.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();
            return PartialView("Partials/_CourseRow", c);
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _db.Courses
                .Include(c => c.PrerequisiteCourse)
                .Include(c => c.Coordinator).ThenInclude(l => l.User)
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();
            // if AJAX partial requested, return details partial that includes modules
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Partials/_CourseDetails", course);
            }

            return View(course);
        }

        // GET: Courses/Create
        public async Task<IActionResult> Create()
        {
            // populate prerequisite dropdown from existing courses
            var list = await _db.Courses.OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.PrerequisiteList = new SelectList(list, "Id", "CourseName");
            // populate coordinator list (active lecturers)
            var lecturers = await _db.Lecturers.Include(l => l.User).Where(l => l.User != null && l.User.IsActive).OrderBy(l => l.FirstName).ThenBy(l => l.LastName).ToListAsync();
            var lecList = lecturers.Select(l => new { Id = l.Id, Name = l.User?.FullName ?? (l.FirstName + " " + l.LastName) }).ToList();
            ViewBag.CoordinatorList = new SelectList(lecList, "Id", "Name");
            // also supply departments for selection
            var depts = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Departments = new SelectList(depts, "Id", "DepartmentName");
            return View();
        }

        // GET: Courses/CreateModal (partial for AJAX)
        [HttpGet]
        public async Task<IActionResult> CreateModal()
        {
            var list = await _db.Courses.OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.PrerequisiteList = new SelectList(list, "Id", "CourseName");
            var lecturers = await _db.Lecturers.Include(l => l.User).Where(l => l.User != null && l.User.IsActive).OrderBy(l => l.FirstName).ThenBy(l => l.LastName).ToListAsync();
            var lecList = lecturers.Select(l => new { Id = l.Id, Name = l.User?.FullName ?? (l.FirstName + " " + l.LastName) }).ToList();
            ViewBag.CoordinatorList = new SelectList(lecList, "Id", "Name");
            var depts = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Departments = new SelectList(depts, "Id", "DepartmentName");
            return PartialView("Partials/_CourseForm", new Course());
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseCode,CourseName,Description,Credits,PrerequisiteCourseId")] Course course)
        {
            // remove navigation property validation
            ModelState.Remove("PrerequisiteCourse");

            // map PrerequisiteCourseId explicitly if present
            if (Request?.Form != null && Request.Form.ContainsKey("PrerequisiteCourseId"))
            {
                var raw = Request.Form["PrerequisiteCourseId"].FirstOrDefault();
                if (int.TryParse(raw, out var pid)) course.PrerequisiteCourseId = pid;
                else course.PrerequisiteCourseId = null;
            }
            // map CoordinatorId explicitly from form
            if (Request?.Form != null && Request.Form.ContainsKey("CoordinatorId"))
            {
                var rawc = Request.Form["CoordinatorId"].FirstOrDefault();
                if (int.TryParse(rawc, out var cid)) course.CoordinatorId = cid;
                else course.CoordinatorId = null;
            }
            // map IsActive if present
            if (Request?.Form != null && Request.Form.ContainsKey("IsActive"))
            {
                var rawActive = Request.Form["IsActive"].FirstOrDefault();
                if (rawActive == "1" || rawActive?.ToLower() == "true")
                    course.IsActive = true;
                else
                    course.IsActive = false;
            }
            // map CoordinatorId explicitly
            if (Request?.Form != null && Request.Form.ContainsKey("CoordinatorId"))
            {
                var rawc = Request.Form["CoordinatorId"].FirstOrDefault();
                if (int.TryParse(rawc, out var cid)) course.CoordinatorId = cid;
                else course.CoordinatorId = null;
            }

            // self-prerequisite guard
            if (course.PrerequisiteCourseId != null && course.PrerequisiteCourseId == course.Id)
            {
                ModelState.AddModelError("PrerequisiteCourseId", "A course cannot be its own prerequisite.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ensure default active
                    course.IsActive = true;
                    // map department if provided
                    if (Request?.Form != null && Request.Form.ContainsKey("DepartmentId"))
                    {
                        var rawd = Request.Form["DepartmentId"].FirstOrDefault();
                        if (int.TryParse(rawd, out var did)) course.DepartmentId = did;
                        else course.DepartmentId = null;
                    }

                    _db.Add(course);
                    await _db.SaveChangesAsync();
                    // Always return JSON success for modal-based flows (caller handles navigation)
                    return Json(new { success = true, id = course.Id });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CoursesController.Create Save error: {ex}");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the course.");
                }
            }

            var list = await _db.Courses.OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.PrerequisiteList = new SelectList(list, "Id", "CourseName", course.PrerequisiteCourseId);
            var depts = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Departments = new SelectList(depts, "Id", "DepartmentName", course.DepartmentId);
            // If this was an AJAX request (modal), return the partial form with validation errors
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Partials/_CourseForm", course);
            }
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // populate prerequisites excluding self
            var list = await _db.Courses.Where(c => c.Id != id).OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.PrerequisiteList = new SelectList(list, "Id", "CourseName", course.PrerequisiteCourseId);
            // coordinator list
            var lecturers = await _db.Lecturers.Include(l => l.User).Where(l => l.User != null && l.User.IsActive).OrderBy(l => l.FirstName).ThenBy(l => l.LastName).ToListAsync();
            var lecList = lecturers.Select(l => new { Id = l.Id, Name = l.User?.FullName ?? (l.FirstName + " " + l.LastName) }).ToList();
            ViewBag.CoordinatorList = new SelectList(lecList, "Id", "Name", course.CoordinatorId);
            var depts = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Departments = new SelectList(depts, "Id", "DepartmentName", course.DepartmentId);
            return View(course);
        }

        // GET: Courses/EditModal/5
        [HttpGet]
        public async Task<IActionResult> EditModal(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();
            var list = await _db.Courses.Where(c => c.Id != id).OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.PrerequisiteList = new SelectList(list, "Id", "CourseName", course.PrerequisiteCourseId);
            var lecturers = await _db.Lecturers.Include(l => l.User).Where(l => l.User != null && l.User.IsActive).OrderBy(l => l.FirstName).ThenBy(l => l.LastName).ToListAsync();
            var lecList = lecturers.Select(l => new { Id = l.Id, Name = l.User?.FullName ?? (l.FirstName + " " + l.LastName) }).ToList();
            ViewBag.CoordinatorList = new SelectList(lecList, "Id", "Name", course.CoordinatorId);
            var depts = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Departments = new SelectList(depts, "Id", "DepartmentName", course.DepartmentId);
            return PartialView("Partials/_CourseForm", course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseCode,CourseName,Description,Credits,PrerequisiteCourseId")] Course course)
        {
            if (id != course.Id) return NotFound();

            ModelState.Remove("PrerequisiteCourse");

            if (Request?.Form != null && Request.Form.ContainsKey("PrerequisiteCourseId"))
            {
                var raw = Request.Form["PrerequisiteCourseId"].FirstOrDefault();
                if (int.TryParse(raw, out var pid)) course.PrerequisiteCourseId = pid;
                else course.PrerequisiteCourseId = null;
            }

            // cannot be own prerequisite
            if (course.PrerequisiteCourseId != null && course.PrerequisiteCourseId == course.Id)
            {
                ModelState.AddModelError("PrerequisiteCourseId", "A course cannot be its own prerequisite.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _db.Courses.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.CourseCode = course.CourseCode;
                    existing.CourseName = course.CourseName;
                    existing.Description = course.Description;
                    existing.Credits = course.Credits;
                    existing.PrerequisiteCourseId = course.PrerequisiteCourseId;
                    // map coordinator
                    if (Request?.Form != null && Request.Form.ContainsKey("CoordinatorId"))
                    {
                        var rawc = Request.Form["CoordinatorId"].FirstOrDefault();
                        if (int.TryParse(rawc, out var cid)) existing.CoordinatorId = cid;
                        else existing.CoordinatorId = null;
                    }
                    // map department
                    if (Request?.Form != null && Request.Form.ContainsKey("DepartmentId"))
                    {
                        var rawd = Request.Form["DepartmentId"].FirstOrDefault();
                        if (int.TryParse(rawd, out var did)) existing.DepartmentId = did;
                        else existing.DepartmentId = null;
                    }
                    existing.IsActive = course.IsActive;

                    _db.Update(existing);
                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CourseExists(course.Id)) return NotFound();
                        throw;
                    }
                    // Return JSON so modal caller can hide and refresh
                    return Json(new { success = true, id = existing.Id });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CoursesController.Edit Save error: {ex}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }

            var list = await _db.Courses.Where(c => c.Id != id).OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.PrerequisiteList = new SelectList(list, "Id", "CourseName", course.PrerequisiteCourseId);
            var depts = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Departments = new SelectList(depts, "Id", "DepartmentName", course.DepartmentId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Partials/_CourseForm", course);
            }
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _db.Courses
                .Include(c => c.PrerequisiteCourse)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course != null)
            {
                _db.Courses.Remove(course);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Placeholder: check if a student can enroll in a course (has passed prerequisite)
        private bool CanStudentEnroll(int studentId, int courseId)
        {
            var course = _db.Courses.Find(courseId);
            if (course == null) return false;
            if (course.PrerequisiteCourseId == null) return true;

            var pid = course.PrerequisiteCourseId.Value;
            // simple check: student has an enrollment record for prerequisite with status 'Passed' or 'Completed'
            return _db.Enrollments.Any(e => e.StudentId == studentId && e.CourseId == pid && (e.Status.ToLower() == "passed" || e.Status.ToLower() == "completed"));
        }

        private bool CourseExists(int id)
        {
            return _db.Courses.Any(e => e.Id == id);
        }
    }
}
