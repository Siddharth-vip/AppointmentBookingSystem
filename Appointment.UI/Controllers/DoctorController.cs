using Microsoft.AspNetCore.Mvc;
using Appointment.UI.Services;
using Appointment.UI.Models;
using System.Globalization;

namespace Appointment.UI.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApiService api;

        public DoctorController(ApiService apiService)
        {
            api = apiService;
        }

        // ===============================
        // SHOW ALL DOCTORS
        // ===============================
        public async Task<IActionResult> Index()
        {
            var doctors = await api.GetDoctorsAsync();

            if (doctors == null)
                doctors = new List<Doctor>();

            return View(doctors);
        }

        // ===============================
        // CHECK DOCTOR AVAILABILITY
        // ===============================
        [HttpPost]
public async Task<IActionResult> CheckAvailability(DateTime date, string time)
{
    // Convert time string to TimeSpan
    TimeSpan parsedTime = TimeSpan.Parse(time);

    var doctors = await api.GetAvailableDoctorsAsync(date, parsedTime);

    if (doctors == null || doctors.Count == 0)
    {
        ViewBag.Message = "No one is there at this time";
        return View("NoDoctor");
    }

    return View("Index", doctors);
}

        // ===============================
        // VIEW DOCTOR SLOTS
        // ===============================
        public async Task<IActionResult> ViewSlots(int doctorId)
        {
            var slots = await api.GetTimeSlotsAsync(doctorId);

            if (slots == null)
                slots = new List<TimeSlot>();

            return View(slots);
        }
    }
}