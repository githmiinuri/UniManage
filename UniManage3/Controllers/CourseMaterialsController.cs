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
    public class CourseMaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const string UploadsFolder = "uploads/materials";

        public CourseMaterialsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: CourseMaterials
        public async Task<IActionResult> Index()
        {
            var materials = await _context.CourseMaterials.Include(c => c.Module).ToListAsync();
            return View(materials);
        }

        // GET: CourseMaterials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.CourseMaterials.Include(c => c.Module).FirstOrDefaultAsync(m => m.Id == id);
            if (material == null) return NotFound();

            return View(material);
        }

        // GET: CourseMaterials/Create
        public IActionResult Create()
        {
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName");
            ViewData["MaterialTypes"] = Enum.GetValues(typeof(MaterialTypeEnum)).Cast<MaterialTypeEnum>().Select(m => new SelectListItem { Text = m.ToString(), Value = m.ToString() }).ToList();
            return View();
        }

        // POST: CourseMaterials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseMaterialUploadViewModel vm)
        {
            ModelState.Remove("FilePath");

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName", vm.ModuleId);
            ViewData["MaterialTypes"] = Enum.GetValues(typeof(MaterialTypeEnum)).Cast<MaterialTypeEnum>().Select(m => new SelectListItem { Text = m.ToString(), Value = m.ToString() }).ToList();

            if (!ModelState.IsValid)
            {
                Debug.WriteLine("ModelState is invalid in Create action (initial). Errors:");
                foreach (var kvp in ModelState)
                {
                    var key = kvp.Key;
                    var errors = kvp.Value.Errors;
                    foreach (var err in errors)
                    {
                        Debug.WriteLine($" - {key}: {err.ErrorMessage}");
                    }
                }

                return View(vm);
            }

            if (vm.UploadedFile == null || vm.UploadedFile.Length == 0)
            {
                Debug.WriteLine("UploadedFile is null or empty in Create action.");
                ModelState.AddModelError("UploadedFile", "Please select a file to upload.");
                return View(vm);
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", UploadsFolder.Replace('/', Path.DirectorySeparatorChar));
            try
            {
                if (!Directory.Exists(uploadsRoot))
                {
                    Debug.WriteLine($"Creating uploads directory: {uploadsRoot}");
                    Directory.CreateDirectory(uploadsRoot);
                }

                var ext = Path.GetExtension(vm.UploadedFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadsRoot, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await vm.UploadedFile.CopyToAsync(stream);
                }

                var relativePath = $"/{UploadsFolder}/{fileName}";

                Debug.WriteLine($"Created relativePath: {relativePath}");

                var material = new CourseMaterial
                {
                    MaterialName = vm.MaterialName,
                    MaterialType = vm.MaterialType,
                    Description = vm.Description,
                    Duration = vm.Duration,
                    FilePath = relativePath,
                    ModuleId = vm.ModuleId
                };

                if (!ModelState.IsValid)
                {
                    Debug.WriteLine("ModelState is invalid in Create action after assigning FilePath. Errors:");
                    foreach (var kvp in ModelState)
                    {
                        var key = kvp.Key;
                        var errors = kvp.Value.Errors;
                        foreach (var err in errors)
                        {
                            Debug.WriteLine($" - {key}: {err.ErrorMessage}");
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Validation failed after assigning file path.");
                    return View(vm);
                }

                Debug.WriteLine("Adding material to DbContext");
                _context.Add(material);

                try
                {
                    Debug.WriteLine("Calling SaveChangesAsync for Create");
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("SaveChangesAsync succeeded for Create");
                }
                catch (DbUpdateException dbEx)
                {
                    var msg = dbEx.InnerException?.Message ?? dbEx.Message;
                    Debug.WriteLine($"DbUpdateException on SaveChangesAsync in Create: {msg}");
                    ModelState.AddModelError(string.Empty, "Database error: " + msg);
                    return View(vm);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General exception in Create action: {ex}");
                ModelState.AddModelError(string.Empty, "An error occurred while uploading the file: " + ex.Message);
                return View(vm);
            }
        }

        // GET: CourseMaterials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null) return NotFound();

            var vm = new CourseMaterialUploadViewModel
            {
                Id = material.Id,
                MaterialName = material.MaterialName,
                MaterialType = material.MaterialType,
                Description = material.Description,
                Duration = material.Duration,
                FilePath = material.FilePath,
                ModuleId = material.ModuleId
            };

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName", vm.ModuleId);
            ViewData["MaterialTypes"] = Enum.GetValues(typeof(MaterialTypeEnum)).Cast<MaterialTypeEnum>().Select(m => new SelectListItem { Text = m.ToString(), Value = m.ToString() }).ToList();

            return View(vm);
        }

        // POST: CourseMaterials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseMaterialUploadViewModel vm)
        {
            ModelState.Remove("FilePath");

            if (id != vm.Id) return NotFound();

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName", vm.ModuleId);
            ViewData["MaterialTypes"] = Enum.GetValues(typeof(MaterialTypeEnum)).Cast<MaterialTypeEnum>().Select(m => new SelectListItem { Text = m.ToString(), Value = m.ToString() }).ToList();

            if (!ModelState.IsValid)
            {
                Debug.WriteLine("ModelState is invalid in Edit action (initial). Errors:");
                foreach (var kvp in ModelState)
                {
                    var key = kvp.Key;
                    var errors = kvp.Value.Errors;
                    foreach (var err in errors)
                    {
                        Debug.WriteLine($" - {key}: {err.ErrorMessage}");
                    }
                }

                return View(vm);
            }

            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null) return NotFound();

            try
            {
                if (vm.UploadedFile != null && vm.UploadedFile.Length > 0)
                {
                    var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", UploadsFolder.Replace('/', Path.DirectorySeparatorChar));
                    if (!Directory.Exists(uploadsRoot))
                    {
                        Debug.WriteLine($"Creating uploads directory: {uploadsRoot}");
                        Directory.CreateDirectory(uploadsRoot);
                    }

                    if (!string.IsNullOrEmpty(material.FilePath))
                    {
                        var oldPath = material.FilePath.TrimStart('/').Replace(',', Path.DirectorySeparatorChar);
                        var oldFull = Path.Combine(_env.WebRootPath ?? "wwwroot", oldPath);
                        if (System.IO.File.Exists(oldFull))
                        {
                            System.IO.File.Delete(oldFull);
                        }
                    }

                    var ext = Path.GetExtension(vm.UploadedFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var fullPath = Path.Combine(uploadsRoot, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await vm.UploadedFile.CopyToAsync(stream);
                    }

                    material.FilePath = $"/{UploadsFolder}/{fileName}";
                }

                material.MaterialName = vm.MaterialName;
                material.MaterialType = vm.MaterialType;
                material.Description = vm.Description;
                material.Duration = vm.Duration;
                material.ModuleId = vm.ModuleId;

                if (!ModelState.IsValid)
                {
                    Debug.WriteLine("ModelState is invalid in Edit action after updates. Errors:");
                    foreach (var kvp in ModelState)
                    {
                        var key = kvp.Key;
                        var errors = kvp.Value.Errors;
                        foreach (var err in errors)
                        {
                            Debug.WriteLine($" - {key}: {err.ErrorMessage}");
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Model validation failed after updates. See debug output for details.");
                    return View(vm);
                }

                Debug.WriteLine("Updating material in DbContext (Edit)");
                _context.Update(material);

                try
                {
                    Debug.WriteLine("Calling SaveChangesAsync for Edit");
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("SaveChangesAsync succeeded for Edit");
                }
                catch (DbUpdateException dbEx)
                {
                    var msg = dbEx.InnerException?.Message ?? dbEx.Message;
                    Debug.WriteLine($"DbUpdateException on SaveChangesAsync in Edit: {msg}");
                    ModelState.AddModelError(string.Empty, "Database error: " + msg);
                    return View(vm);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseMaterialExists(vm.Id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General exception in Edit action: {ex}");
                ModelState.AddModelError(string.Empty, "An error occurred while saving changes: " + ex.Message);
                return View(vm);
            }
        }

        // GET: CourseMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.CourseMaterials.Include(c => c.Module).FirstOrDefaultAsync(m => m.Id == id);
            if (material == null) return NotFound();

            return View(material);
        }

        // POST: CourseMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null) return RedirectToAction(nameof(Index));

            try
            {
                if (!string.IsNullOrEmpty(material.FilePath))
                {
                    var path = material.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var full = Path.Combine(_env.WebRootPath ?? "wwwroot", path);
                    if (System.IO.File.Exists(full))
                    {
                        System.IO.File.Delete(full);
                    }
                }

                _context.CourseMaterials.Remove(material);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the material: " + ex.Message);
                return View(material);
            }
        }

        // GET: CourseMaterials/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null || string.IsNullOrEmpty(material.FilePath)) return NotFound();

            var path = material.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var full = Path.Combine(_env.WebRootPath ?? "wwwroot", path);
            if (!System.IO.File.Exists(full)) return NotFound();

            var contentType = GetContentType(full);
            var fileName = Path.GetFileName(full);
            return PhysicalFile(full, contentType, fileName);
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }

        private bool CourseMaterialExists(int id)
        {
            return _context.CourseMaterials.Any(e => e.Id == id);
        }
    }
}
