using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace UniManage3.Controllers
{
    public class ModulesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ModulesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Modules
        public async Task<IActionResult> Index()
        {
            var modules = _db.Modules.Include(m => m.Course);
            return View(await modules.ToListAsync());
        }

        // GET: Modules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var module = await _db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == id);
            if (module == null) return NotFound();
            return View(module);
        }

        // GET: Modules/Create (admin)
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_db.Courses, "Id", "CourseName", null);
            return View();
        }

        // GET: Modules/Create?courseId=5 (AJAX modal)
        public IActionResult CreateForCourse(int courseId)
        {
            return NotFound();
        }

        // POST: Modules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ModuleCode,ModuleName,Description,Credits,CourseId,LecturerId")] Module module, [FromForm] List<Module> Modules, int? CourseId)
        {
            ModelState.Remove("Lecturer");
            ModelState.Remove("Assignments");
            ModelState.Remove("CourseMaterials");
            if (ModelState.IsValid)
            {
                try
                {
                    if (Modules != null && Modules.Any())
                    {
                        foreach (var m in Modules)
                        {
                            if (CourseId.HasValue) m.CourseId = CourseId.Value;
                            m.IsActive = true;
                            _db.Modules.Add(m);
                        }
                        await _db.SaveChangesAsync();
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(new { success = true });
                        return RedirectToAction(nameof(Index));
                    }

                    module.IsActive = true;
                    _db.Modules.Add(module);
                    await _db.SaveChangesAsync();
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ModulesController.Create Save error: {ex}");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the module.");
                }
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }
            ViewData["CourseId"] = new SelectList(_db.Courses, "Id", "CourseName", module.CourseId);
            return View(module);
        }

        // GET: Modules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var module = await _db.Modules.FindAsync(id);
            if (module == null) return NotFound();
            ViewData["CourseId"] = new SelectList(_db.Courses, "Id", "CourseName", module.CourseId);
            return View(module);
        }

        // POST: Modules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ModuleCode,ModuleName,Description,Credits,CourseId,LecturerId")] Module module)
        {
            if (id != module.Id) return NotFound();
            ModelState.Remove("Lecturer");
            ModelState.Remove("Assignments");
            ModelState.Remove("CourseMaterials");
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _db.Modules.FirstOrDefaultAsync(m => m.Id == id);
                    if (existing == null) return NotFound();
                    existing.ModuleCode = module.ModuleCode;
                    existing.ModuleName = module.ModuleName;
                    existing.Description = module.Description;
                    existing.Credits = module.Credits;
                    existing.CourseId = module.CourseId;
                    existing.LecturerId = module.LecturerId;
                    _db.Update(existing);
                    await _db.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.Modules.Any(e => e.Id == module.Id)) return NotFound();
                    throw;
                }
            }
            ViewData["CourseId"] = new SelectList(_db.Courses, "Id", "CourseName", module.CourseId);
            return View(module);
        }

        // GET: Modules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var module = await _db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == id);
            if (module == null) return NotFound();
            return View(module);
        }

        // POST: Modules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var module = await _db.Modules.FindAsync(id);
            if (module != null)
            {
                _db.Modules.Remove(module);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ModuleExists(int id)
        {
            return _db.Modules.Any(e => e.Id == id);
        }

        // POST: Modules/ToggleActive/5 (AJAX)
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var module = await _db.Modules.FindAsync(id);
            if (module == null) return Json(new { success = false, message = "Not found" });
            module.IsActive = !module.IsActive;
            _db.Update(module);
            await _db.SaveChangesAsync();
            return Json(new { success = true, isActive = module.IsActive });
        }

        // POST: Modules/DeleteAjax/5 (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var module = await _db.Modules.FindAsync(id);
            if (module == null) return Json(new { success = false, message = "Not found" });
            _db.Modules.Remove(module);
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

    }
}