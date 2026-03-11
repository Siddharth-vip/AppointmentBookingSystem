using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Appointment.API.Repository;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        DoctorRepository doctorRepo = new DoctorRepository();
        TimeSlotRepository slotRepo = new TimeSlotRepository();

        // TOTAL DOCTORS
        [Authorize(Roles = "Admin")]
        [HttpGet("total-doctors")]
        public IActionResult GetTotalDoctors()
        {
            int total = doctorRepo.GetTotalDoctors();

            return Ok(total);
        }

        // TOTAL APPOINTMENTS
        [Authorize(Roles = "Admin")]
        [HttpGet("total-appointments")]
        public IActionResult GetTotalAppointments()
        {
            int total = slotRepo.GetTotalAppointments();

            return Ok(total);
        }
    }
}