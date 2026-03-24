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
        public async Task<IActionResult> Dashboard(DateTime? selectedDate)
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            if (doctorId == null)
            {
                return RedirectToAction("Login");
            }

            DateTime dashboardDate = selectedDate?.Date ?? DateTime.Today;

            var allSlots = await api.GetTimeSlotsAsync(doctorId.Value);
            var allAppointments = await api.GetDoctorAppointmentsAsync(doctorId.Value);

            var model = new DoctorDashboardViewModel
            {
                SelectedDate = dashboardDate,
                Slots = allSlots
                    .Where(s => s.SlotDate.Date == dashboardDate)
                    .OrderBy(s => s.StartTime)
                    .ToList(),
                Appointments = allAppointments
                    .Where(a => a.SlotDate.Date == dashboardDate)
                    .OrderBy(a => a.StartTime)
                    .ToList()
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
            if (date.Date < DateTime.Today)
            {
                ViewBag.Message = "Please select today or a future date.";
                return View("NoDoctor");
            }

            TimeSpan parsedTime = TimeSpan.Parse(time);

            var doctors = await api.GetAvailableDoctorsAsync(date, parsedTime);

            if (doctors == null || doctors.Count == 0)
            {
                ViewBag.Message = "No one is there at this time";
                return View("NoDoctor");
            }

            ViewBag.IsFromSearch = true;
            ViewBag.SelectedDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ViewBag.SelectedTime = parsedTime.ToString(@"hh\:mm");

            return View("Index", doctors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookFromSearch(int doctorId, DateTime selectedDate, string selectedTime)
        {
            if (!TimeSpan.TryParse(selectedTime, out TimeSpan desiredTime))
            {
                TempData["ErrorMessage"] = "Invalid time selected.";
                return RedirectToAction("Index");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth", new
                {
                    returnUrl = Url.Action("Book", "Appointment", new { doctorId })
                });
            }

            var slots = await api.GetTimeSlotsAsync(doctorId);

            var matchedSlot = slots.FirstOrDefault(slot =>
            {
                if (slot.IsBooked || slot.SlotDate.Date != selectedDate.Date)
                {
                    return false;
                }

                if (!DateTime.TryParse(slot.StartTime, out DateTime parsedStart))
                {
                    return false;
                }

                return parsedStart.TimeOfDay == desiredTime;
            });

            if (matchedSlot == null)
            {
                TempData["ErrorMessage"] = "Selected time slot is no longer available. Please choose another slot.";
                return RedirectToAction("Book", "Appointment", new { doctorId });
            }

            var appointment = new Appointment.UI.Models.Appointment
            {
                UserId = userId.Value,
                DoctorId = doctorId,
                SlotId = matchedSlot.SlotId,
                AppointmentDate = DateTime.Now,
                Status = "Booked"
            };

            var bookingResult = await api.BookAppointmentAsync(appointment);

            if (bookingResult.Success)
            {
                TempData["SuccessMessage"] = bookingResult.Message;
                TempData["JustBookedSlotId"] = matchedSlot.SlotId;
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = bookingResult.Message +
                (bookingResult.SmsSent ? " SMS notification sent to your number." : "");

            return RedirectToAction("Book", "Appointment", new { doctorId });
        }

        // ===============================
        // VIEW DOCTOR SLOTS
        // ===============================
        public async Task<IActionResult> ViewSlots(int doctorId)
        {
            var slots = await api.GetTimeSlotsAsync(doctorId);

            if (slots == null)
                slots = new List<TimeSlot>();

            slots = slots
                .Where(slot => slot.SlotDate.Date >= DateTime.Today)
                .OrderBy(slot => slot.SlotDate)
                .ThenBy(slot => slot.StartTime)
                .ToList();

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
                TempData["JustBookedSlotId"] = slotId;
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