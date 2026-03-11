using Microsoft.AspNetCore.Mvc;
using Appointment.API.Repository;

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

        // BOOK APPOINTMENT
        [HttpPost("book")]
        public IActionResult BookAppointment(int userId, int doctorId, int slotId)
        {
            DateTime appointmentDate = DateTime.Now;

            repo.BookAppointment(userId, doctorId, slotId, appointmentDate);

            return Ok(new
            {
                message = "Appointment booked successfully"
            });
        }

        // GET ALL APPOINTMENTS
        [HttpGet("all")]
        public IActionResult GetAllAppointments()
        {
            var list = repo.GetAllAppointments();
            return Ok(list);
        }

        // USER APPOINTMENTS
        [HttpGet("user/{userId}")]
        public IActionResult GetUserAppointments(int userId)
        {
            var list = repo.GetAppointmentsByUser(userId);
            return Ok(list);
        }

        // DOCTOR APPOINTMENTS
        [HttpGet("doctor/{doctorId}")]
        public IActionResult GetDoctorAppointments(int doctorId)
        {
            var list = repo.GetAppointmentsByDoctor(doctorId);
            return Ok(list);
        }
    }
}