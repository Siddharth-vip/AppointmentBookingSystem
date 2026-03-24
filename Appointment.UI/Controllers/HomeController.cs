using Microsoft.AspNetCore.Mvc;
using Appointment.UI.Services;
using Appointment.UI.Models;
using System.Linq;

namespace Appointment.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Landing Page
        public async Task<IActionResult> Index()
        {
            var model = new HomeIndexViewModel();

            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            model.IsLoggedIn = userId.HasValue;
            model.IsAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            model.UserName = HttpContext.Session.GetString("UserName") ?? "User";

            if (userId.HasValue && !model.IsAdmin)
            {
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
                    return string.IsNullOrWhiteSpace(status) ? "Booked" : status.Trim();
                }

                var normalized = appointments
                    .Select(a =>
                    {
                        a.Status = NormalizeStatus(a.Status);
                        return a;
                    })
                    .OrderByDescending(a => a.SlotDate)
                    .ThenByDescending(a => a.StartTime)
                    .ToList();

                model.TotalBookedCount = normalized.Count(a => !unavailableStatuses.Contains(a.Status ?? string.Empty));
                model.TotalUnavailableCount = normalized.Count(a => unavailableStatuses.Contains(a.Status ?? string.Empty));
                model.RecentBookedSlots = normalized
                    .Where(a => !unavailableStatuses.Contains(a.Status ?? string.Empty))
                    .Take(3)
                    .ToList();
                model.RecentUnavailableSlots = normalized
                    .Where(a => unavailableStatuses.Contains(a.Status ?? string.Empty))
                    .Take(3)
                    .ToList();

                if (TempData["JustBookedSlotId"] != null
                    && int.TryParse(TempData["JustBookedSlotId"]?.ToString(), out int justBookedSlotId))
                {
                    var justBooked = normalized.FirstOrDefault(a => a.SlotId == justBookedSlotId);
                    if (justBooked != null)
                    {
                        model.ShowBookedJustNow = true;
                        model.BookedJustNow = justBooked;
                    }
                }
            }

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}