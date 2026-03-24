using Microsoft.AspNetCore.Mvc;
using Appointment.API.Repository;
using Appointment.API.Models;
using Appointment.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentRepository repo;
        private readonly SmsService smsService;
        private readonly UserRepository userRepo;

        public AppointmentController()
        {
            repo = new AppointmentRepository();
            smsService = new SmsService();
            userRepo = new UserRepository();
        }

        // ===============================
        // BOOK APPOINTMENT (FIXED)
        // ===============================
        [Authorize]
        [HttpPost("book")]
        public IActionResult BookAppointment([FromBody] Appointment.API.Models.Appointment appointment)
        {
            string? email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var loggedInUser = userRepo.GetUserByEmail(email);
            if (loggedInUser == null)
            {
                return Unauthorized(new { message = "User not found for token." });
            }

            // Always trust JWT user instead of client payload for secure ownership.
            appointment.UserId = loggedInUser.UserId;

            if (appointment.SlotId == 0)
            {
                return BadRequest("SlotId is 0 - Data not received properly");
            }

            var result = repo.BookAppointment(appointment);

            if (!result.Success && result.ErrorCode == "SLOT_UNAVAILABLE")
            {
                string? phone = repo.GetUserPhone(appointment.UserId);
                bool smsSent = smsService.SendDoctorUnavailableMessage(phone);

                return Conflict(new
                {
                    message = result.Message,
                    smsSent
                });
            }

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(new
            {
                message = result.Message
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