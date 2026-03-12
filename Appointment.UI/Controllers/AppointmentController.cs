using Microsoft.AspNetCore.Mvc;
using Appointment.UI.Services;
using Appointment.UI.Models;

namespace Appointment.UI.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApiService api;

        public AppointmentController(ApiService apiService)
        {
            api = apiService;
        }

        // ===============================
        // SHOW TIME SLOTS
        // ===============================
        public async Task<IActionResult> Book(int doctorId)
{
    var slots = await api.GetTimeSlotsAsync(doctorId);

    if (slots == null)
        slots = new List<TimeSlot>();

    ViewBag.Slots = slots;

    var appointment = new Appointment.UI.Models.Appointment
{
    DoctorId = doctorId
};

    return View(appointment);
}

        // ===============================
        // BOOK APPOINTMENT
        // ===============================
        [HttpPost]
        public async Task<IActionResult> BookAppointment(int doctorId, int slotId)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            // USER NOT LOGGED IN
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await api.GetUserByEmailAsync(email);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Create appointment object
            var appointment = new Appointment.UI.Models.Appointment
            {
                UserId = user.UserId,
                DoctorId = doctorId,
                SlotId = slotId,
                AppointmentDate = DateTime.Now,
                Status = "Booked"
            };

            // Call API
            await api.BookAppointmentAsync(appointment);

            return RedirectToAction("Confirmed");
        }

        // ===============================
        // CONFIRMATION PAGE
        // ===============================
        public IActionResult Confirmed()
        {
            return View();
        }
    }
}