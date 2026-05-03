using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;

namespace UniManage3.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DepartmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Departments
        public async Task<IActionResult> Index(string searchTerm = null)
        {
            // Sync with sidebar active state
            ViewBag.ActiveAction = "Departments";

            await PopulateActiveLecturersSelectList();

            var query = _db.Departments
                .Include(d => d.HeadOfDepartment)
                    .ThenInclude(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.Trim();
                query = query.Where(d => d.DepartmentCode.Contains(s));
                ViewBag.SearchTerm = s;
            }

            var departments = await query.OrderBy(d => d.DepartmentName).ToListAsync();

            return View("Departments", departments);
        }

        // Modal partials for AJAX-driven UI
        [HttpGet]
        public async Task<IActionResult> CreateModal()
        {
            await PopulateActiveLecturersSelectList();
            return PartialView("Partials/_DepartmentForm", new Department());
        }

        [HttpGet]
        public async Task<IActionResult> EditModal(int id)
        {
            var department = await _db.Departments.FindAsync(id);
            if (department == null) return NotFound();
            await PopulateActiveLecturersSelectList(department.HeadOfDepartmentId);
            return PartialView("Partials/_DepartmentForm", department);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsModal(int id)
        {
            var department = await _db.Departments
                .Include(d => d.HeadOfDepartment).ThenInclude(l => l.User)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (department == null) return NotFound();
            return PartialView("Partials/_DepartmentDetails", department);
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var department = await _db.Departments
                .Include(d => d.HeadOfDepartment)
                    .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();

            return View(department);
        }

        // GET: Departments/Create
        public async Task<IActionResult> Create()
        {
            await PopulateActiveLecturersSelectList();
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartmentName,DepartmentCode,Description,HeadOfDepartmentId")] Department department)
        {
            var code = (department.DepartmentCode ?? string.Empty).Trim();

            // Check uniqueness
            if (await _db.Departments.AnyAsync(d => d.DepartmentCode.ToLower() == code.ToLower()))
            {
                ModelState.AddModelError("DepartmentCode", "Department code must be unique.");
            }

            // CRITICAL: Remove navigation property validation to prevent silent failure
            ModelState.Remove("HeadOfDepartment");

            if (ModelState.IsValid)
            {
                department.DepartmentCode = code;
                department.CreatedAt = DateTime.Now;
                department.UpdatedAt = DateTime.Now;

                _db.Add(department);
                try
                {
                    await _db.SaveChangesAsync();

                    // If AJAX call (Modal), return JSON success
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Create Error: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the department.");
                }
            }

            // If we got here, something failed; repopulate and return
            await PopulateActiveLecturersSelectList(department.HeadOfDepartmentId);
            return PartialView("Partials/_DepartmentForm", department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var department = await _db.Departments.FindAsync(id);
            if (department == null) return NotFound();

            await PopulateActiveLecturersSelectList(department.HeadOfDepartmentId);
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DepartmentName,DepartmentCode,Description,HeadOfDepartmentId")] Department department)
        {
            if (id != department.Id) return NotFound();

            var code = (department.DepartmentCode ?? string.Empty).Trim();
            if (await _db.Departments.AnyAsync(d => d.Id != department.Id && d.DepartmentCode.ToLower() == code.ToLower()))
            {
                ModelState.AddModelError("DepartmentCode", "Department code must be unique.");
            }

            // CRITICAL: Remove navigation property validation
            ModelState.Remove("HeadOfDepartment");

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _db.Departments.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.DepartmentName = department.DepartmentName;
                    existing.DepartmentCode = code;
                    existing.Description = department.Description;
                    existing.HeadOfDepartmentId = department.HeadOfDepartmentId;
                    existing.UpdatedAt = DateTime.Now;

                    _db.Update(existing);
                    await _db.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Edit Error: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the department.");
                }
            }

            await PopulateActiveLecturersSelectList(department.HeadOfDepartmentId);
            return PartialView("Partials/_DepartmentForm", department);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var department = await _db.Departments
                .Include(d => d.HeadOfDepartment)
                    .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _db.Departments.FindAsync(id);
            if (department != null)
            {
                _db.Departments.Remove(department);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _db.Departments.Any(e => e.Id == id);
        }

        // Helper: Populate list of active lecturers for dropdown
        private async Task PopulateActiveLecturersSelectList(int? selectedId = null)
        {
            var lecturers = await _db.Lecturers
                .Include(l => l.User)
                .Where(l => l.User != null && l.User.IsActive)
                .OrderBy(l => l.FirstName).ThenBy(l => l.LastName)
                .ToListAsync();

            var list = lecturers.Select(l => new {
                Id = l.Id,
                Name = l.User?.FullName ?? (l.FirstName + " " + l.LastName)
            }).ToList();

            ViewBag.Lecturers = new SelectList(list, "Id", "Name", selectedId);

            var items = lecturers.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = l.User?.FullName ?? (l.FirstName + " " + l.LastName),
                Selected = (selectedId != null && l.Id == selectedId)
            }).ToList();
            ViewBag.ActiveLecturers = items;
        }
    }
}
