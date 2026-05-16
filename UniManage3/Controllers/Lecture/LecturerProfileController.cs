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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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

        // GET: Lecture/LecturerProfile/Index
        public async Task<IActionResult> Index()
        {
            var vm = new LecturerProfileIndexViewModel();

            var email = GetCurrentLoginEmail();
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
                vm.Email.CurrentEmail = user.Email;
                return View(vm);
            }

            PopulateViewModel(vm, user, lecturer);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "Profile")] ProfileUpdateViewModel model)
        {
            var email = GetCurrentLoginEmail();
            if (string.IsNullOrEmpty(email)) return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to locate your user profile.");
                return ReturnIndexView(new LecturerProfileIndexViewModel { Profile = model });
            }

            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user.Id);
            if (lecturer == null)
            {
                ModelState.AddModelError(string.Empty, "No lecturer profile found for the current user.");
                var vmMissing = new LecturerProfileIndexViewModel { Profile = model };
                vmMissing.Email.CurrentEmail = user.Email;
                return ReturnIndexView(vmMissing);
            }

            if (!ModelState.IsValid)
            {
                var vm = new LecturerProfileIndexViewModel();
                PopulateViewModel(vm, user, lecturer);
                vm.Profile = model;
                return ReturnIndexView(vm);
            }

            lecturer.FirstName = model.FirstName?.Trim();
            lecturer.LastName = model.LastName?.Trim();
            if (int.TryParse(model.ContactNumber, out var contactParsed))
            {
                lecturer.ContactNumber = contactParsed;
            }
            else
            {
                lecturer.ContactNumber = null;
            }
            lecturer.NICNumber = model.NICNumber?.Trim();
            lecturer.AddressLine1 = model.AddressLine1?.Trim();
            lecturer.AddressLine2 = model.AddressLine2?.Trim();
            lecturer.Province = model.Province?.Trim();
            lecturer.City = model.City?.Trim();
            lecturer.ZipCode = model.ZipCode;

            user.FullName = (model.FirstName + " " + model.LastName).Trim();

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. " + ex.Message);
                var vm = new LecturerProfileIndexViewModel();
                PopulateViewModel(vm, user, lecturer);
                vm.Profile = model;
                return ReturnIndexView(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail([Bind(Prefix = "Email")] EmailUpdateViewModel model)
        {
            var email = GetCurrentLoginEmail();
            if (string.IsNullOrEmpty(email)) return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to locate your user profile.");
                return ReturnIndexView(new LecturerProfileIndexViewModel { Email = model });
            }

            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user.Id);
            if (lecturer == null)
            {
                ModelState.AddModelError(string.Empty, "No lecturer profile found for the current user.");
                var vmMissing = new LecturerProfileIndexViewModel { Email = model };
                vmMissing.Email.CurrentEmail = model.CurrentEmail ?? user.Email;
                return ReturnIndexView(vmMissing);
            }

            if (!ModelState.IsValid)
            {
                var vm = new LecturerProfileIndexViewModel();
                PopulateViewModel(vm, user, lecturer);
                vm.Email = model;
                return ReturnIndexView(vm);
            }

            if (!string.Equals(model.CurrentEmail?.Trim(), user.Email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Email.CurrentEmail", "Current email does not match our records.");
                var vm = new LecturerProfileIndexViewModel();
                PopulateViewModel(vm, user, lecturer);
                vm.Email = model;
                return ReturnIndexView(vm);
            }

            var newEmailTrimmed = model.NewEmail.Trim();
            var exists = await _context.Users.AnyAsync(u => u.Email == newEmailTrimmed && u.Id != user.Id);
            if (exists)
            {
                ModelState.AddModelError("Email.NewEmail", "The new email is already in use by another account.");
                var vm = new LecturerProfileIndexViewModel();
                PopulateViewModel(vm, user, lecturer);
                vm.Email = model;
                return ReturnIndexView(vm);
            }

            user.Email = newEmailTrimmed;
            try
            {
                await _context.SaveChangesAsync();
                // Sign the user out so they must sign in again using the new email address
                TempData["SuccessMessage"] = "Email updated successfully. Please sign in with your new email.";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unable to update email. " + ex.Message);
                var vm = new LecturerProfileIndexViewModel();
                PopulateViewModel(vm, user, lecturer);
                vm.Email = model;
                return ReturnIndexView(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "Password")] PasswordUpdateViewModel model)
        {
            var email = GetCurrentLoginEmail();
            if (string.IsNullOrEmpty(email)) return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to locate your user profile.");
                return ReturnIndexView(new LecturerProfileIndexViewModel { Password = model });
            }

            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user.Id);
            if (lecturer == null)
            {
                ModelState.AddModelError(string.Empty, "No lecturer profile found for the current user.");
                var vmMissing = new LecturerProfileIndexViewModel { Password = model };
                vmMissing.Email.CurrentEmail = user.Email;
                return ReturnIndexView(vmMissing);
            }

            if (!ModelState.IsValid)
            {
                var vm = new LecturerProfileIndexViewModel();
                PopulateViewModel(vm, user, lecturer);
                vm.Password = model;
                return ReturnIndexView(vm);
            }

            if (!VerifyHashedPassword(user.Password, model.CurrentPassword))
            {
                ModelState.AddModelError("Password.CurrentPassword", "Current password is incorrect.");
                var vm = new LecturerProfileIndexViewModel();
                PopulateViewModel(vm, user, lecturer);
                vm.Password = model;
                return ReturnIndexView(vm);
            }

            user.Password = HashPassword(model.NewPassword);
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unable to change password. " + ex.Message);
                var vm = new LecturerProfileIndexViewModel();
                PopulateViewModel(vm, user, lecturer);
                vm.Password = model;
                return ReturnIndexView(vm);
            }
        }

        private IActionResult ReturnIndexView(LecturerProfileIndexViewModel vm) =>
            View("Index", vm);

        private string? GetCurrentLoginEmail() =>
            User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.Email)?.Value;

        private void PopulateViewModel(LecturerProfileIndexViewModel vm, User user, Lecturer lecturer)
        {
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
