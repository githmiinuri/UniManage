using Microsoft.AspNetCore.Mvc;
using UniManage3.Data;
using UniManage3.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;


namespace UniManage3.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, bool remember)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return View();
            }

            var hasher = new PasswordHasher<object>();

            // Ensure there is a matching user record and that it's approved
            var user = _db.Users.FirstOrDefault(u => u.Email == username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password");
                return View();
            }

            if (!user.IsApproved)
            {
                ModelState.AddModelError(string.Empty, "Your account is pending approval by an administrator.");
                return View();
            }

            var verifyResult = hasher.VerifyHashedPassword(null, user.Password, password);
            if (verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                // Map role id to role name
                string roleName = "";
                switch (user.RoleId)
                {
                    case 1:
                        roleName = "Administrator";
                        break;
                    case 2:
                        roleName = "Lecturer";
                        break;
                    case 3:
                        roleName = "Student";
                        break;
                    default:
                        roleName = "User";
                        break;
                }

                // Update last login and activate if needed
                user.LastLogin = DateTime.UtcNow;
                user.IsActive = true;
                _db.SaveChanges();

                await SignInUser(user.Email, roleName, remember);
                // Redirect administrators to the Admin dashboard
                if (roleName == "Administrator")
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password");
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Registration()
        {
            return View("Registration", new Models.RegistrationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Registration(Models.RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Registration", model);
            }

            var hasher = new PasswordHasher<object>();
            var passwordHash = hasher.HashPassword(null, model.Password);

            var role = model.Role;

            if (role == "Lecturer")
            {
                var exists = _db.Lecturers.Any(l => l.Email == model.Email);
                if (exists) { ModelState.AddModelError(string.Empty, "Email already registered"); return View("Registration", model); }
                if (!int.TryParse(model.ContactNumber, out var contactInt))
                {
                    ModelState.AddModelError("ContactNumber", "Contact number must be numeric.");
                    return View("Registration", model);
                }

                int zipInt = 0;
                if (!string.IsNullOrWhiteSpace(model.ZipCode) && !int.TryParse(model.ZipCode, out zipInt))
                {
                    ModelState.AddModelError("ZipCode", "Zip code must be numeric.");
                    return View("Registration", model);
                }

                var lect = new Lecturer
                {
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    RoleId = 2,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    Province = model.Province,
                    City = model.City,
                    ZipCode = zipInt,
                    ContactNumber = contactInt,
                    NICNumber = model.NICNumber
                };
                _db.Lecturers.Add(lect);
                _db.SaveChanges();

                // Create a pending user record (requires admin approval before login)
                var user = new User
                {
                    FullName = model.FirstName + " " + model.LastName,
                    Email = model.Email,
                    Password = passwordHash,
                    RoleId = 2,
                    IsActive = false,
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = null
                };
                _db.Users.Add(user);
                _db.SaveChanges();
                ViewData["Role"] = "Lecturer";
                return View("RegistrationSuccess");
            }

            if (role == "Student")
            {
                var exists = _db.Students.Any(s => s.Email == model.Email);
                if (exists) { ModelState.AddModelError(string.Empty, "Email already registered"); return View("Registration", model); }
                if (!int.TryParse(model.ContactNumber, out var contactInt2))
                {
                    ModelState.AddModelError("ContactNumber", "Contact number must be numeric.");
                    return View("Registration", model);
                }

                int zipInt2 = 0;
                if (!string.IsNullOrWhiteSpace(model.ZipCode) && !int.TryParse(model.ZipCode, out zipInt2))
                {
                    ModelState.AddModelError("ZipCode", "Zip code must be numeric.");
                    return View("Registration", model);
                }

                var stud = new Student
                {
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    RoleId = 3,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    Province = model.Province,
                    City = model.City,
                    ZipCode = zipInt2,
                    ContactNumber = contactInt2,
                    NICNumber = model.NICNumber
                };
                _db.Students.Add(stud);
                _db.SaveChanges();

                // Create a pending user record (requires admin approval before login)
                var user = new User
                {
                    FullName = model.FirstName + " " + model.LastName,
                    Email = model.Email,
                    Password = passwordHash,
                    RoleId = 3,
                    IsActive = false,
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = null
                };
                _db.Users.Add(user);
                _db.SaveChanges();
                ViewData["Role"] = "Student";
                return View("RegistrationSuccess");
            }

            if (role == "Administrator")
            {
                var exists = _db.Administrators.Any(a => a.Email == model.Email);
                if (exists) { ModelState.AddModelError(string.Empty, "Email already registered"); return View("Registration", model); }
                if (!int.TryParse(model.ContactNumber, out var contactInt3))
                {
                    ModelState.AddModelError("ContactNumber", "Contact number must be numeric.");
                    return View("Registration", model);
                }

                int zipInt3 = 0;
                if (!string.IsNullOrWhiteSpace(model.ZipCode) && !int.TryParse(model.ZipCode, out zipInt3))
                {
                    ModelState.AddModelError("ZipCode", "Zip code must be numeric.");
                    return View("Registration", model);
                }

                var adm = new Administrator
                {
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    RoleId = 1,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    Province = model.Province,
                    City = model.City,
                    ZipCode = zipInt3,
                    ContactNumber = contactInt3,
                    NICNumber = model.NICNumber
                };
                _db.Administrators.Add(adm);
                _db.SaveChanges();

                // Create a pending user record (requires admin approval before login)
                var user = new User
                {
                    FullName = model.FirstName + " " + model.LastName,
                    Email = model.Email,
                    Password = passwordHash,
                    RoleId = 1,
                    IsActive = false,
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = null
                };
                _db.Users.Add(user);
                _db.SaveChanges();
                ViewData["Role"] = "Administrator";
                return View("RegistrationSuccess");
            }

            ModelState.AddModelError(string.Empty, "Unknown role");
            return View();
        }

        private async Task SignInUser(string email, string role, bool remember)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = remember
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
