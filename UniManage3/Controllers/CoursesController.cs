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
        public async Task<IActionResult> Index(string searchString)
        {
            var courses = await _db.Courses
            // base query with includes
            var query = _db.Courses
                .Include(c => c.PrerequisiteCourse)
                .Include(c => c.Department)
                .Include(c => c.Coordinator).ThenInclude(l => l.User)
                .AsQueryable();

            // apply search filter when provided
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c => c.CourseName.Contains(searchString));
                ViewBag.SearchTerm = searchString;
            }

            var courses = await query.OrderBy(c => c.CourseName).ToListAsync();

            // calculate inactive count explicitly from boolean IsActive
            var inactiveCount = await _db.Courses.CountAsync(c => c.IsActive == false);
            ViewBag.InactiveCount = inactiveCount;

            return View("Courses", courses);
        }

        // GET: Courses/Row/5 - returns a partial table row for AJAX refresh
        [HttpGet]
        public async Task<IActionResult> Row(int id)
        {
            var c = await _db.Courses
                .Include(x => x.PrerequisiteCourse)
                .Include(x => x.Coordinator).ThenInclude(l => l.User)
                .Include(x => x.Department)
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
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Partials/_CourseDetails", course);
            }
            return View(course);
        }

        // GET: Courses/Create
        public async Task<IActionResult> Create()
        {
            var list = await _db.Courses.OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.PrerequisiteList = new SelectList(list, "Id", "CourseName");
            var lecturers = await _db.Lecturers.Include(l => l.User).Where(l => l.User != null && l.User.IsActive).OrderBy(l => l.FirstName).ThenBy(l => l.LastName).ToListAsync();
            var lecList = lecturers.Select(l => new { Id = l.Id, Name = l.User?.FullName ?? (l.FirstName + " " + l.LastName) }).ToList();
            ViewBag.CoordinatorList = new SelectList(lecList, "Id", "Name");
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
        public async Task<IActionResult> Create([Bind("Id,CourseCode,CourseName,Description,Credits,PrerequisiteCourseId,CoordinatorId,DepartmentId,IsActive")] Course course)
        {
            // remove navigation property validation so binding by id passes
            ModelState.Remove("PrerequisiteCourse");
            ModelState.Remove("Coordinator");
            ModelState.Remove("Department");
            // modules is a navigation collection that will be null on initial POST - don't validate it here
            ModelState.Remove("Modules");

            if (Request?.Form != null && Request.Form.ContainsKey("PrerequisiteCourseId"))
            {
                var raw = Request.Form["PrerequisiteCourseId"].FirstOrDefault();
                if (int.TryParse(raw, out var pid)) course.PrerequisiteCourseId = pid;
                else course.PrerequisiteCourseId = null;
            }
            if (Request?.Form != null && Request.Form.ContainsKey("CoordinatorId"))
            {
                var rawc = Request.Form["CoordinatorId"].FirstOrDefault();
                if (int.TryParse(rawc, out var cid)) course.CoordinatorId = cid;
                else course.CoordinatorId = null;
            }
            if (Request?.Form != null && Request.Form.ContainsKey("IsActive"))
            {
                var rawActive = Request.Form["IsActive"].FirstOrDefault();
                if (rawActive == "1" || rawActive?.ToLower() == "true")
                    course.IsActive = true;
                else
                    course.IsActive = false;
            }

            if (course.PrerequisiteCourseId != null && course.PrerequisiteCourseId == course.Id)
            {
                ModelState.AddModelError("PrerequisiteCourseId", "A course cannot be its own prerequisite.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Request?.Form != null && Request.Form.ContainsKey("DepartmentId"))
                    {
                        var rawd = Request.Form["DepartmentId"].FirstOrDefault();
                        if (int.TryParse(rawd, out var did)) course.DepartmentId = did;
                        else course.DepartmentId = null;
                    }

                    // Do not process Modules here — module management is decoupled
                    _db.Add(course);
                    await _db.SaveChangesAsync();
                    // Always return JSON success for modal flows; include id so callers can refresh the row
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
            var deptsList = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Departments = new SelectList(deptsList, "Id", "DepartmentName", course.DepartmentId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _db.Courses.Include(c => c.Modules).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            var list = await _db.Courses.Where(c => c.Id != id).OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.PrerequisiteList = new SelectList(list, "Id", "CourseName", course.PrerequisiteCourseId);
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
            var course = await _db.Courses.Include(c => c.Modules).FirstOrDefaultAsync(c => c.Id == id);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseCode,CourseName,Description,Credits,PrerequisiteCourseId,CoordinatorId,DepartmentId,IsActive")] Course course)
        {
            if (id != course.Id) return NotFound();

            // remove navigation validation - we only post IDs
            ModelState.Remove("PrerequisiteCourse");
            ModelState.Remove("Coordinator");
            ModelState.Remove("Department");
            // modules is a navigation collection that will be null on initial POST - don't validate it here
            ModelState.Remove("Modules");

            if (Request?.Form != null && Request.Form.ContainsKey("PrerequisiteCourseId"))
            {
                var raw = Request.Form["PrerequisiteCourseId"].FirstOrDefault();
                if (int.TryParse(raw, out var pid)) course.PrerequisiteCourseId = pid;
                else course.PrerequisiteCourseId = null;
            }

            if (course.PrerequisiteCourseId != null && course.PrerequisiteCourseId == course.Id)
            {
                ModelState.AddModelError("PrerequisiteCourseId", "A course cannot be its own prerequisite.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ensure the course exists
                    if (!CourseExists(course.Id)) return NotFound();

                    // read coordinator/department from form (IDs)
                    if (Request?.Form != null && Request.Form.ContainsKey("CoordinatorId"))
                    {
                        var rawc = Request.Form["CoordinatorId"].FirstOrDefault();
                        if (int.TryParse(rawc, out var cid)) course.CoordinatorId = cid;
                        else course.CoordinatorId = null;
                    }
                    if (Request?.Form != null && Request.Form.ContainsKey("DepartmentId"))
                    {
                        var rawd = Request.Form["DepartmentId"].FirstOrDefault();
                        if (int.TryParse(rawd, out var did)) course.DepartmentId = did;
                        else course.DepartmentId = null;
                    }

                    // Persist the bound course entity
                    // First, load existing tracked course with its modules
                    var existing = await _db.Courses.Include(c => c.Modules).FirstOrDefaultAsync(c => c.Id == id);
                    if (existing == null) return NotFound();

                    // Update scalar properties
                    existing.CourseCode = course.CourseCode;
                    existing.CourseName = course.CourseName;
                    existing.Description = course.Description;
                    existing.Credits = course.Credits;
                    existing.PrerequisiteCourseId = course.PrerequisiteCourseId;
                    existing.CoordinatorId = course.CoordinatorId;
                    existing.DepartmentId = course.DepartmentId;
                    existing.IsActive = course.IsActive;

                    // Handle modules collection from posted form (model binder will populate course.Modules when inputs are named correctly)
                    var postedModules = course.Modules ?? new List<Module>();

                    // Update existing modules and collect ids
                    var postedIds = new HashSet<int>(postedModules.Where(m => m.Id != 0).Select(m => m.Id));

                    // Remove modules that were removed on the form
                    var toRemove = existing.Modules.Where(m => !postedIds.Contains(m.Id)).ToList();
                    foreach (var rem in toRemove)
                    {
                        _db.Modules.Remove(rem);
                    }

                    // Update or add posted modules
                    foreach (var pm in postedModules)
                    {
                        if (pm.Id != 0)
                        {
                            var existMod = existing.Modules.FirstOrDefault(m => m.Id == pm.Id);
                            if (existMod != null)
                            {
                                existMod.ModuleCode = pm.ModuleCode;
                                existMod.ModuleName = pm.ModuleName;
                                existMod.Description = pm.Description;
                                existMod.Credits = pm.Credits;
                                existMod.IsActive = pm.IsActive;
                                // ensure FK
                                existMod.CourseId = existing.Id;
                                // mark module as modified so EF will persist the changes
                                _db.Update(existMod);
                            }
                        }
                        else
                        {
                            // new module
                            pm.CourseId = existing.Id;
                            _db.Modules.Add(pm);
                        }
                    }
                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CourseExists(course.Id)) return NotFound();
                        throw;
                    }
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CoursesController.Edit Save error: {ex}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }

            var list2 = await _db.Courses.Where(c => c.Id != id).OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.PrerequisiteList = new SelectList(list2, "Id", "CourseName", course.PrerequisiteCourseId);
            var depts2 = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Departments = new SelectList(depts2, "Id", "DepartmentName", course.DepartmentId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
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

        private bool CourseExists(int id)
        {
            return _db.Courses.Any(e => e.Id == id);
        }
    }
}
