using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UniManage3.Controllers
{
    public class CurriculumController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CurriculumController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var activeCourses = await _db.Courses.Where(c => c.IsActive).ToListAsync();
            ViewBag.TotalActiveCourses = activeCourses.Count;
            return View(activeCourses);
        }

        public async Task<IActionResult> ManageCourse(int courseId)
        {
            var course = await _db.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var viewModel = new CourseCurriculumViewModel
            {
                Course = course,
                Semesters = await _db.Semesters
                    .Where(s => s.CourseId == courseId)
                    .Include(s => s.Modules)
                    .OrderBy(s => s.StartDate)
                    .ToListAsync(),
                UnassignedModules = await _db.Modules
                    .Where(m => m.CourseId == courseId && m.SemesterId == null && m.IsActive)
                    .ToListAsync()
            };

            return PartialView("_CourseDetail", viewModel);
        }

        // ==========================================
        // SEMESTER ACTIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetSemesterDetails(int id)
        {
            var semester = await _db.Semesters.FindAsync(id);
            if (semester == null) return NotFound();
            return PartialView("_SemesterDetails", semester);
        }

        [HttpGet]
        public async Task<IActionResult> GetSemesterEdit(int id)
        {
            var semester = await _db.Semesters.FindAsync(id);
            if (semester == null) return NotFound();
            return PartialView("_SemesterEdit", semester);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSemester(Semester model)
        {
            ModelState.Remove("Course");
            if (ModelState.IsValid)
            {
                _db.Semesters.Update(model);
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = "Invalid data" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSemester(int id)
        {
            var semester = await _db.Semesters.Include(s => s.Modules).FirstOrDefaultAsync(s => s.Id == id);
            if (semester == null) return Json(new { success = false });

            // Unassign modules instead of deleting them to prevent data loss
            foreach (var mod in semester.Modules)
            {
                mod.SemesterId = null;
            }

            _db.Semesters.Remove(semester);
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ==========================================
        // MODULE ACTIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetSemesterModules(int semesterId)
        {
            var modules = await _db.Modules
                .Include(m => m.Lecturer)
                .Where(m => m.SemesterId == semesterId)
                .ToListAsync();
            return PartialView("_SemesterModuleList", modules);
        }

        [HttpGet]
        public async Task<IActionResult> GetModuleDetails(int id)
        {
            var module = await _db.Modules
                .Include(m => m.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (module == null) return NotFound();
            return PartialView("_ModuleDetails", module);
        }

        [HttpGet]
        public async Task<IActionResult> GetModuleEdit(int id)
        {
            var module = await _db.Modules.FindAsync(id);
            if (module == null) return NotFound();

            var lecturers = await _db.Lecturers
                .Select(l => new { l.Id, FullName = l.FirstName + " " + l.LastName })
                .ToListAsync();

            ViewBag.Lecturers = new SelectList(lecturers, "Id", "FullName", module.LecturerId);
            return PartialView("_ModuleEdit", module);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModule(Module model)
        {
            // Clear navigation properties for validation
            ModelState.Clear();

            // Re-validate only the fields we care about
            if (string.IsNullOrEmpty(model.ModuleName) || string.IsNullOrEmpty(model.ModuleCode))
                return Json(new { success = false, errors = "Name and Code are required." });

            var existing = await _db.Modules.FindAsync(model.Id);
            if (existing == null) return Json(new { success = false });

            // Update only allowed fields
            existing.ModuleCode = model.ModuleCode;
            existing.ModuleName = model.ModuleName;
            existing.LecturerId = model.LecturerId;
            existing.Credits = model.Credits;
            existing.Description = model.Description;

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ==========================================
        // HELPER / EXISTING ACTIONS
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSemester(Semester model)
        {
            ModelState.Remove("Course");
            if (ModelState.IsValid)
            {
                _db.Semesters.Add(model);
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = "Validation failed" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModule(Module model)
        {
            ModelState.Clear(); // Skip heavy model binding validation for nav properties
            model.IsActive = true;
            _db.Modules.Add(model);
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveLecturers()
        {
            var lecturers = await _db.Lecturers
                .Select(l => new { l.Id, Name = l.FirstName + " " + l.LastName })
                .ToListAsync();
            return Json(lecturers);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleModuleStatus(int id, bool active)
        {
            var module = await _db.Modules.FindAsync(id);
            if (module == null) return Json(new { success = false });
            module.IsActive = active;
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteModule(int id)
        {
            var module = await _db.Modules.FindAsync(id);
            if (module == null) return Json(new { success = false });
            _db.Modules.Remove(module);
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
