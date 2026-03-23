using Microsoft.AspNetCore.Mvc;
using Appointment.API.Repository;
using Appointment.API.Models;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentRepository repo;

        public AppointmentController()
        {
            repo = new AppointmentRepository();
        }

        // ===============================
        // BOOK APPOINTMENT (FIXED)
        // ===============================
        [HttpPost("book")]
        public IActionResult BookAppointment([FromBody] Appointment.API.Models.Appointment appointment)
        {
            Console.WriteLine($"🔥 SlotId received: {appointment.SlotId}");

            if (appointment.SlotId == 0)
            {
                return BadRequest("SlotId is 0 - Data not received properly");
            }

            // Save appointment
            repo.BookAppointment(appointment);

            return Ok(new
            {
                message = "Appointment booked successfully"
            });
        }

        // ===============================
        // GET ALL APPOINTMENTS
        // ===============================
        [HttpGet("all")]
        public IActionResult GetAllAppointments()
        {
            var list = repo.GetAllAppointments();
            return Ok(list);
        }

        // ===============================
        // USER APPOINTMENTS
        // ===============================
        [HttpGet("user/{userId}")]
        public IActionResult GetUserAppointments(int userId)
        {
            var list = repo.GetAppointmentsByUser(userId);
            return Ok(list);
        }

        // ===============================
        // DOCTOR APPOINTMENTS
        // ===============================
        [HttpGet("doctor/{doctorId}")]
        public IActionResult GetDoctorAppointments(int doctorId)
        {
            var list = repo.GetAppointmentsByDoctor(doctorId);
            return Ok(list);
        }
    }
}