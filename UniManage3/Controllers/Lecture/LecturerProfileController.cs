using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using UniManage3.Data;
using UniManage3.Models;
using UniManage3.Models.ViewModels;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UniManage3.Controllers.Lecture
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LecturerProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Lecture/Profile
        public async Task<IActionResult> Index()
        {
            var vm = new LecturerProfileIndexViewModel();

            // identify current user by email claim or name
            var email = User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to locate your user profile.";
                return View(vm);
            }

            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user.Id);
            if (lecturer == null)
            {
                TempData["ErrorMessage"] = "No lecturer profile found for the current user.";
                return View(vm);
            }

            vm.Profile.FirstName = lecturer.FirstName;
            vm.Profile.LastName = lecturer.LastName;
            vm.Profile.ContactNumber = lecturer.ContactNumber?.ToString();
            vm.Profile.NICNumber = lecturer.NICNumber;
            vm.Profile.AddressLine1 = lecturer.AddressLine1;
            vm.Profile.AddressLine2 = lecturer.AddressLine2;
            vm.Profile.Province = lecturer.Province;
            vm.Profile.City = lecturer.City;
            vm.Profile.ZipCode = lecturer.ZipCode;

            vm.Email.CurrentEmail = user.Email;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the highlighted errors.";
                return RedirectToAction(nameof(Index));
            }

            var email = User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to locate your user profile.";
                return RedirectToAction(nameof(Index));
            }

            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user.Id);
            if (lecturer == null)
            {
                TempData["ErrorMessage"] = "No lecturer profile found for the current user.";
                return RedirectToAction(nameof(Index));
            }

            // update lecturer fields
            lecturer.FirstName = model.FirstName;
            lecturer.LastName = model.LastName;
            if (int.TryParse(model.ContactNumber, out var contactParsed))
            {
                lecturer.ContactNumber = contactParsed;
            }
            else
            {
                lecturer.ContactNumber = null;
            }
            lecturer.NICNumber = model.NICNumber;
            lecturer.AddressLine1 = model.AddressLine1;
            lecturer.AddressLine2 = model.AddressLine2;
            lecturer.Province = model.Province;
            lecturer.City = model.City;
            lecturer.ZipCode = model.ZipCode;

            // update user's full name
            user.FullName = (model.FirstName + " " + model.LastName).Trim();

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unable to save changes. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail(EmailUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the highlighted errors.";
                return RedirectToAction(nameof(Index));
            }

            var email = User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to locate your user profile.";
                return RedirectToAction(nameof(Index));
            }

            // verify current email matches
            if (!string.Equals(model.CurrentEmail?.Trim(), user.Email, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Current email does not match our records.";
                return RedirectToAction(nameof(Index));
            }

            // check unique
            var exists = await _context.Users.AnyAsync(u => u.Email == model.NewEmail && u.Id != user.Id);
            if (exists)
            {
                TempData["ErrorMessage"] = "The new email is already in use by another account.";
                return RedirectToAction(nameof(Index));
            }

            user.Email = model.NewEmail.Trim();
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Email updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unable to update email. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(PasswordUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the highlighted errors.";
                return RedirectToAction(nameof(Index));
            }

            var email = User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to locate your user profile.";
                return RedirectToAction(nameof(Index));
            }

            // verify current password - users.Password assumed hashed with SHA256 or similar
            if (!VerifyHashedPassword(user.Password, model.CurrentPassword))
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return RedirectToAction(nameof(Index));
            }

            user.Password = HashPassword(model.NewPassword);
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Password changed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unable to change password. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        private static bool VerifyHashedPassword(string hashed, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashed) || string.IsNullOrEmpty(providedPassword)) return false;
            var providedHash = HashPassword(providedPassword);
            return string.Equals(hashed, providedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
