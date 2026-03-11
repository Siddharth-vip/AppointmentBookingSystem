using Microsoft.AspNetCore.Mvc;
using Appointment.API.Repository;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeSlotController : ControllerBase
    {
        private readonly TimeSlotRepository repo = new TimeSlotRepository();

        // GET SLOTS FOR A DOCTOR
        [HttpGet("doctor/{doctorId}")]
        public IActionResult GetSlotsByDoctor(int doctorId)
        {
            var slots = repo.GetSlotsByDoctor(doctorId);

            return Ok(slots);
        }
    }
}