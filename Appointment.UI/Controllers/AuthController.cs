using Microsoft.AspNetCore.Mvc;
using Appointment.UI.Services;
using Appointment.UI.Models;

namespace Appointment.UI.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _apiService;

        public AuthController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // ===============================
        // LOGIN PAGE
        // ===============================
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ===============================
        // LOGIN POST
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var loginResponse = await _apiService.LoginUserAsync(email, password);

            if (loginResponse?.User != null)
            {
                var user = loginResponse.User;

                // Save session values
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", loginResponse.Role ?? user.Role ?? "User");

                if (!string.IsNullOrWhiteSpace(loginResponse.Token))
                {
                    HttpContext.Session.SetString("JwtToken", loginResponse.Token);
                }

                // If login came from booking page
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if (string.Equals(loginResponse.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                // Default redirect
                return RedirectToAction("Index", "Home");
            }

            // Login failed
            ViewBag.Error = "Invalid email or password";
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // ===============================
        // REGISTER PAGE
        // ===============================
        public IActionResult Register()
        {
            return View();
        }

        // ===============================
        // REGISTER POST
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            await _apiService.RegisterUserAsync(user);

            return RedirectToAction("Login");
        }

        // ===============================
        // LOGOUT (FIXED)
        // ===============================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            // Always go to login page after logout
            return RedirectToAction("Login");
        }
    }
}