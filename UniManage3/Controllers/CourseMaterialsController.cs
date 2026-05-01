using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName", vm.ModuleId);
            ViewData["MaterialTypes"] = Enum.GetValues(typeof(MaterialTypeEnum)).Cast<MaterialTypeEnum>().Select(m => new SelectListItem { Text = m.ToString(), Value = m.ToString() }).ToList();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.UploadedFile == null || vm.UploadedFile.Length == 0)
            {
                ModelState.AddModelError("UploadedFile", "Please select a file to upload.");
                return View(vm);
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", UploadsFolder.Replace('/', Path.DirectorySeparatorChar));
            try
            {
                if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                var ext = Path.GetExtension(vm.UploadedFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadsRoot, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await vm.UploadedFile.CopyToAsync(stream);
                }

                var relativePath = $"/{UploadsFolder}/{fileName}"; // for storing in DB

                var material = new CourseMaterial
                {
                    MaterialName = vm.MaterialName,
                    MaterialType = vm.MaterialType,
                    Description = vm.Description,
                    Duration = vm.Duration,
                    FilePath = relativePath,
                    ModuleId = vm.ModuleId
                };

                _context.Add(material);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
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
            if (id != vm.Id) return NotFound();

            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "ModuleName", vm.ModuleId);
            ViewData["MaterialTypes"] = Enum.GetValues(typeof(MaterialTypeEnum)).Cast<MaterialTypeEnum>().Select(m => new SelectListItem { Text = m.ToString(), Value = m.ToString() }).ToList();

            if (!ModelState.IsValid) return View(vm);

            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null) return NotFound();

            try
            {
                // Handle file replacement
                if (vm.UploadedFile != null && vm.UploadedFile.Length > 0)
                {
                    var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", UploadsFolder.Replace('/', Path.DirectorySeparatorChar));
                    if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                    // delete old file
                    if (!string.IsNullOrEmpty(material.FilePath))
                    {
                        var oldPath = material.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                        var oldFull = Path.Combine(_env.WebRootPath ?? "wwwroot", oldPath);
                        if (System.IO.File.Exists(oldFull))
                        {
                            System.IO.File.Delete(oldFull);
                        }
                    }

                    // save new file
                    var ext = Path.GetExtension(vm.UploadedFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var fullPath = Path.Combine(uploadsRoot, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await vm.UploadedFile.CopyToAsync(stream);
                    }

                    material.FilePath = $"/{UploadsFolder}/{fileName}";
                }

                // update other properties
                material.MaterialName = vm.MaterialName;
                material.MaterialType = vm.MaterialType;
                material.Description = vm.Description;
                material.Duration = vm.Duration;
                material.ModuleId = vm.ModuleId;

                _context.Update(material);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseMaterialExists(vm.Id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
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
                // delete file from disk
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
