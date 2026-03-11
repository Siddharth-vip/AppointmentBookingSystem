using Microsoft.AspNetCore.Mvc;
using Appointment.UI.Services;
using Appointment.UI.Models;

namespace Appointment.UI.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApiService api;

        public DoctorController(ApiService apiService)
        {
            api = apiService;
        }

        // FIND DOCTORS
        public async Task<IActionResult> Find()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Auth");
            }

            var doctors = await api.GetDoctorsAsync();

            if (doctors == null)
            {
                doctors = new List<Doctor>();
            }

            return View(doctors);
        }
    }
}