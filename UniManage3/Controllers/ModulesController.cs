<<<<<<< HEAD
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
using System.Diagnostics;
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;
using Microsoft.AspNetCore.Authorization;
>>>>>>> 355f77b (#1 feat: Implement the lecturer flow)

namespace UniManage3.Controllers
{
    public class ModulesController : Controller
    {
<<<<<<< HEAD
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
=======
        private readonly ApplicationDbContext _context;

        public ModulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Modules
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Modules.Include(c => c.Course).Include(l => l.Lecturer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Modules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @module = await _context.Modules
                .Include(a => a.Course)
                .Include(l => l.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@module == null)
            {
                return NotFound();
            }

            return View(@module);
        }

        // GET: Modules/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id");
            ViewData["LecturerId"] = new SelectList(_context.Lecturers, "Id", "Id");
            return View();
        }

        // POST: Modules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,ModuleCode,ModuleName,CourseId,LecturerId")] Module @module)
        {
                _context.Add(@module);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            //ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id", @module.CourseId);
            //ViewData["LecturerId"] = new SelectList(_context.Lecturers, "Id", "Id", @module.LecturerId);
            //return View(@module);
        }

        // GET: Modules/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @module = await _context.Modules.FindAsync(id);
            if (@module == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "CourseName", @module.CourseId);
            ViewData["LecturerId"] = new SelectList(_context.Lecturers, "Id", "FirstName", @module.LecturerId);
            return View(@module);
        }

        // POST: Modules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ModuleCode,ModuleName,CourseId,LecturerId")] Module @module)
        {
            if (id != @module.Id)
            {
                return NotFound();
            }

                try
                {
                    _context.Update(@module);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModuleExists(@module.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            
            //ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id", @module.CourseId);
            //ViewData["LecturerId"] = new SelectList(_context.Lecturers, "Id", "Id", @module.LecturerId);
            //return View(@module);
        }

        // GET: Modules/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @module = await _context.Modules
                .Include(a => a.Course)
                .Include(l => l.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@module == null)
            {
                return NotFound();
            }

            return View(@module);
        }

        // POST: Modules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @module = await _context.Modules.FindAsync(id);
            if (@module != null)
            {
                _context.Modules.Remove(@module);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ModuleExists(int id)
        {
            return _context.Modules.Any(e => e.Id == id);
>>>>>>> 355f77b (#1 feat: Implement the lecturer flow)
        }
    }
}
