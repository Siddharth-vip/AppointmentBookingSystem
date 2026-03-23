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

        public async Task<IActionResult> Book(int doctorId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

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

        [HttpPost]
        public async Task<IActionResult> BookAppointment([FromForm] int doctorId, [FromForm] int slotId)
        {
            Console.WriteLine($"DoctorId: {doctorId}");
            Console.WriteLine($"SlotId: {slotId}");

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth",
                    new { returnUrl = Url.Action("Book", "Appointment", new { doctorId = doctorId }) });
            }

            if (slotId == 0)
            {
                TempData["ErrorMessage"] = "SlotId is 0. Something went wrong!";
                return RedirectToAction("Book", new { doctorId = doctorId });
            }

            var appointment = new Appointment.UI.Models.Appointment
            {
                UserId = userId.Value,
                DoctorId = doctorId,
                SlotId = slotId,
                AppointmentDate = DateTime.Now,
                Status = "Booked"
            };

            await api.BookAppointmentAsync(appointment);

            TempData["SuccessMessage"] = "Appointment booked successfully!";

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Confirmed()
        {
            return View();
        }
    }
}