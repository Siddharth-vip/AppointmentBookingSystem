using Microsoft.AspNetCore.Mvc;
using Appointment.UI.Models;
using Appointment.UI.Services;

namespace Appointment.UI.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApiService _apiService;

        public AdminController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!string.Equals(HttpContext.Session.GetString("UserRole"), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Auth");
            }

            var doctors = await _apiService.GetDoctorsAsync();
            var model = new AdminDashboardViewModel
            {
                Doctors = doctors
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStandardSlots(int doctorId, DateTime slotDate)
        {
            if (!string.Equals(HttpContext.Session.GetString("UserRole"), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (slotDate.Date < DateTime.Today)
            {
                TempData["AdminError"] = "Please choose today or a future date.";
                return RedirectToAction("Dashboard");
            }

            var result = await _apiService.CreateAdminStandardSlotsAsync(doctorId, slotDate.Date);
            TempData[result.Success ? "AdminSuccess" : "AdminError"] = result.Message;

            return RedirectToAction("Dashboard");
        }
    }
}