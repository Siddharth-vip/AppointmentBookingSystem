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
        // 🔥 DOCTOR LOGIN
        // ===============================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var doctor = await api.LoginDoctorAsync(email, password);

            if (doctor == null)
            {
                ViewBag.Error = "Invalid credentials";
                return View();
            }

            // ✅ Save DoctorId in session
            HttpContext.Session.SetInt32("DoctorId", doctor.DoctorId);
            HttpContext.Session.SetString("DoctorName", doctor.DoctorName ?? "Doctor");

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            if (doctorId == null)
            {
                return RedirectToAction("Login");
            }

            var model = new DoctorDashboardViewModel
            {
                Slots = await api.GetTimeSlotsAsync(doctorId.Value),
                Appointments = await api.GetDoctorAppointmentsAsync(doctorId.Value)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult CreateSlot(DateTime slotDate, string startTime, string endTime)
        {
            TempData["DoctorError"] = "Only admin can create slots.";

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSlot(int slotId)
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            if (doctorId == null)
            {
                return RedirectToAction("Login");
            }

            bool deleted = await api.DeleteDoctorSlotAsync(slotId, doctorId.Value);
            TempData[deleted ? "DoctorSuccess" : "DoctorError"] = deleted
                ? "Slot deleted successfully."
                : "Only unbooked slots can be deleted.";

            return RedirectToAction("Dashboard");
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

        // ===============================
        // BOOK APPOINTMENT
        // ===============================
        public async Task<IActionResult> BookAppointment(int slotId, int doctorId)
        {
            Console.WriteLine("SlotId Received: " + slotId);

            // 🔥 Ensure user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var appointment = new Appointment.UI.Models.Appointment
            {
                UserId = userId.Value,
                DoctorId = doctorId,
                SlotId = slotId,
                AppointmentDate = DateTime.Now,
                Status = "Booked"
            };

            var bookingResult = await api.BookAppointmentAsync(appointment);

            if (bookingResult.Success)
            {
                TempData["SuccessMessage"] = bookingResult.Message;
            }
            else
            {
                TempData["ErrorMessage"] = bookingResult.Message +
                    (bookingResult.SmsSent ? " SMS notification sent to your number." : "");
            }

            return RedirectToAction("Index", "Home");
        }

        // ===============================
        // 🔥 OPTIONAL: DOCTOR LOGOUT
        // ===============================
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("DoctorId");
            HttpContext.Session.Remove("DoctorName");
            return RedirectToAction("Login");
        }
    }
}