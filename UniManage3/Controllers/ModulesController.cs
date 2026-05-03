using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
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

        // GET: Modules/Create?courseId=5
        public IActionResult Create(int courseId)
        {
            var module = new Module { CourseId = courseId };
            return PartialView("Partials/_ModuleForm", module);
        }

        // POST: Modules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ModuleCode,ModuleName,Description,Credits,CourseId")] Module module)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Modules.Add(module);
                    await _db.SaveChangesAsync();
                    // return the course id so the caller can refresh that course row
                    return Json(new { success = true, courseId = module.CourseId });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ModulesController.Create Save error: {ex}");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the module.");
                }
            }
            return PartialView("Partials/_ModuleForm", module);
        }
    }
}
