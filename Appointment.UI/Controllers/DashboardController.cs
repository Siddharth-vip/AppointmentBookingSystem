using Microsoft.AspNetCore.Mvc;
using Appointment.UI.Services;
using Appointment.UI.Models;

namespace Appointment.UI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApiService _apiService;

        public DashboardController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            var appointments = await _apiService.GetUserAppointmentStatusesAsync(userId.Value);

            var unavailableStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Doctor Unavailable",
                "Reschedule Suggested",
                "Not Available",
                "Unavailable"
            };

            static string NormalizeStatus(string? status)
            {
                return string.IsNullOrWhiteSpace(status)
                    ? "Booked"
                    : status.Trim();
            }

            var model = new UserSlotsDashboardViewModel
            {
                UserName = HttpContext.Session.GetString("UserName") ?? "User",
                BookedSlots = appointments
                    .Where(a => !unavailableStatuses.Contains(NormalizeStatus(a.Status)))
                    .Select(a =>
                    {
                        a.Status = NormalizeStatus(a.Status);
                        return a;
                    })
                    .ToList(),
                UnavailableSlots = appointments
                    .Where(a => unavailableStatuses.Contains(NormalizeStatus(a.Status)))
                    .Select(a =>
                    {
                        a.Status = NormalizeStatus(a.Status);
                        return a;
                    })
                    .ToList()
            };

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}