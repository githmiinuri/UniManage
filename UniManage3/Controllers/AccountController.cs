using Microsoft.AspNetCore.Mvc;

namespace UniManage3.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password, bool remember)
        {
            // TODO: add real authentication here.
            // For now, simply redirect back to Login (or to a secured area after successful auth).
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View("Registration");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registration(string email, string role, string password, string confirmPassword)
        {
            // TODO: add real registration logic (validate, create user, etc.).
            // For now, redirect to Login after "successful" registration.
            return RedirectToAction("Login");
        }
    }
}
