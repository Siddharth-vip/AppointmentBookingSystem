using Microsoft.AspNetCore.Mvc;
using Appointment.API.Repository;
using Appointment.API.Models;

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

        [HttpPost("create")]
        public IActionResult CreateSlot([FromBody] TimeSlot slot)
        {
            if (slot.DoctorId <= 0)
                return BadRequest("Invalid doctor.");

            if (slot.SlotDate.Date < DateTime.Today)
                return BadRequest("Slot date cannot be in the past.");

            if (slot.StartTime >= slot.EndTime)
                return BadRequest("Start time must be before end time.");

            bool created = repo.CreateSlot(slot);

            if (!created)
                return BadRequest("Unable to create slot.");

            return Ok(new { message = "Slot created successfully" });
        }

        [HttpDelete("{slotId}")]
        public IActionResult DeleteSlot(int slotId, [FromQuery] int doctorId)
        {
            if (slotId <= 0 || doctorId <= 0)
                return BadRequest("Invalid request.");

            bool deleted = repo.DeleteSlot(slotId, doctorId);

            if (!deleted)
                return BadRequest("Only unbooked slots can be deleted.");

            return Ok(new { message = "Slot deleted successfully" });
        }
    }
}