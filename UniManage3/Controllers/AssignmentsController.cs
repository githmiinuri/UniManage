using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage3.Data;
using UniManage3.Models;

namespace UniManage3.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const string AssignmentsUploadsFolder = "uploads/assignments";

        public AssignmentsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Assignments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Assignments.Include(a => a.Module);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Assignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Module)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // GET: Assignments/Create
        public IActionResult Create()
        {
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName");
            return View();
        }

        // POST: Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentUploadViewModel vm)
        {
            // Ignore server-side required validations for fields we set programmatically
            ModelState.Remove("ResourceFilePath");
            ModelState.Remove("UpdatedDate");

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName", vm.ModuleId);

            if (!ModelState.IsValid)
            {
                Debug.WriteLine("ModelState invalid at start of Create");
                foreach (var kvp in ModelState)
                {
                    foreach (var err in kvp.Value.Errors)
                    {
                        Debug.WriteLine($"{kvp.Key}: {err.ErrorMessage}");
                    }
                }
                return View(vm);
            }

            // Validate uploaded file if present
            if (vm.UploadedFile != null && vm.UploadedFile.Length > 0)
            {
                var ext = Path.GetExtension(vm.UploadedFile.FileName)?.ToLowerInvariant();
                var contentType = vm.UploadedFile.ContentType;
                if (ext != ".pdf" && contentType != "application/pdf")
                {
                    ModelState.AddModelError("UploadedFile", "Only PDF files are allowed.");
                    return View(vm);
                }
            }

            var assignment = new Assignment
            {
                AssignmentName = vm.AssignmentName,
                Description = vm.Description,
                IssuedDate = vm.IssuedDate == default ? DateTime.Now : vm.IssuedDate,
                DeadlineDate = vm.DeadlineDate,
                LateSubmitDate = vm.LateSubmitDate,
                UpdatedDate = null,
                ResourceFilePath = null,
                ModuleId = vm.ModuleId
            };

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", AssignmentsUploadsFolder.Replace('/', Path.DirectorySeparatorChar));
            try
            {
                if (vm.UploadedFile != null && vm.UploadedFile.Length > 0)
                {
                    if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                    var ext = Path.GetExtension(vm.UploadedFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var fullPath = Path.Combine(uploadsRoot, fileName);
                    using (var fs = new FileStream(fullPath, FileMode.Create))
                    {
                        await vm.UploadedFile.CopyToAsync(fs);
                    }

                    assignment.ResourceFilePath = $"/{AssignmentsUploadsFolder}/{fileName}";
                }

                _context.Add(assignment);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    var msg = dbEx.InnerException?.Message ?? dbEx.Message;
                    ModelState.AddModelError(string.Empty, "Database error: " + msg);
                    return View(vm);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the assignment: " + ex.Message);
                return View(vm);
            }
        }

        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();

            var vm = new AssignmentUploadViewModel
            {
                Id = assignment.Id,
                AssignmentName = assignment.AssignmentName,
                Description = assignment.Description,
                IssuedDate = assignment.IssuedDate,
                DeadlineDate = assignment.DeadlineDate,
                LateSubmitDate = assignment.LateSubmitDate,
                UpdatedDate = assignment.UpdatedDate,
                ResourceFilePath = assignment.ResourceFilePath,
                ModuleId = assignment.ModuleId
            };

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName", vm.ModuleId);
            return View(vm);
        }

        // POST: Assignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssignmentUploadViewModel vm)
        {
            ModelState.Remove("ResourceFilePath");
            ModelState.Remove("UpdatedDate");

            if (id != vm.Id) return NotFound();

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName", vm.ModuleId);

            if (!ModelState.IsValid)
            {
                Debug.WriteLine("ModelState invalid at start of Edit");
                foreach (var kvp in ModelState)
                {
                    foreach (var err in kvp.Value.Errors)
                    {
                        Debug.WriteLine($"{kvp.Key}: {err.ErrorMessage}");
                    }
                }
                return View(vm);
            }

            var material = await _context.Assignments.FindAsync(id);
            if (material == null) return NotFound();

            try
            {
                // Handle file replacement
                if (vm.UploadedFile != null && vm.UploadedFile.Length > 0)
                {
                    var ext = Path.GetExtension(vm.UploadedFile.FileName)?.ToLowerInvariant();
                    var contentType = vm.UploadedFile.ContentType;
                    if (ext != ".pdf" && contentType != "application/pdf")
                    {
                        ModelState.AddModelError("UploadedFile", "Only PDF files are allowed.");
                        return View(vm);
                    }

                    var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", AssignmentsUploadsFolder.Replace('/', Path.DirectorySeparatorChar));
                    if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                    // delete old file if exists
                    if (!string.IsNullOrEmpty(material.ResourceFilePath))
                    {
                        var oldPath = material.ResourceFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                        var oldFull = Path.Combine(_env.WebRootPath ?? "wwwroot", oldPath);
                        if (System.IO.File.Exists(oldFull)) System.IO.File.Delete(oldFull);
                    }

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var fullPath = Path.Combine(uploadsRoot, fileName);
                    using (var fs = new FileStream(fullPath, FileMode.Create))
                    {
                        await vm.UploadedFile.CopyToAsync(fs);
                    }

                    material.ResourceFilePath = $"/{AssignmentsUploadsFolder}/{fileName}";
                }

                // update other properties
                material.AssignmentName = vm.AssignmentName;
                material.Description = vm.Description;
                material.IssuedDate = vm.IssuedDate == default ? material.IssuedDate : vm.IssuedDate;
                material.DeadlineDate = vm.DeadlineDate;
                material.LateSubmitDate = vm.LateSubmitDate;
                material.UpdatedDate = DateTime.Now;
                material.ModuleId = vm.ModuleId;

                if (!ModelState.IsValid)
                {
                    Debug.WriteLine("ModelState invalid after updates in Edit");
                    foreach (var kvp in ModelState)
                    {
                        foreach (var err in kvp.Value.Errors)
                        {
                            Debug.WriteLine($"{kvp.Key}: {err.ErrorMessage}");
                        }
                    }
                    return View(vm);
                }

                _context.Update(material);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    var msg = dbEx.InnerException?.Message ?? dbEx.Message;
                    ModelState.AddModelError(string.Empty, "Database error: " + msg);
                    return View(vm);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssignmentExists(vm.Id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the assignment: " + ex.Message);
                return View(vm);
            }
        }

        // GET: Assignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Module)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return RedirectToAction(nameof(Index));

            try
            {
                if (!string.IsNullOrEmpty(assignment.ResourceFilePath))
                {
                    var path = assignment.ResourceFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var full = Path.Combine(_env.WebRootPath ?? "wwwroot", path);
                    if (System.IO.File.Exists(full))
                    {
                        System.IO.File.Delete(full);
                    }
                }

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the assignment: " + ex.Message);
                return View(assignment);
            }
        }

        // GET: Assignments/Download/5
        public IActionResult Download(int id)
        {
            var assignment = _context.Assignments.Find(id);
            if (assignment == null || string.IsNullOrEmpty(assignment.ResourceFilePath)) return NotFound();

            var path = assignment.ResourceFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var full = Path.Combine(_env.WebRootPath ?? "wwwroot", path);
            if (!System.IO.File.Exists(full)) return NotFound();

            return PhysicalFile(full, "application/pdf", Path.GetFileName(full));
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(e => e.Id == id);
        }
    }
}
