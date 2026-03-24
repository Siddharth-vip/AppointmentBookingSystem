using Microsoft.AspNetCore.Mvc;
using Appointment.API.Repository;
using Appointment.API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

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

            var response = slots.Select(slot => new
            {
                slot.SlotId,
                slot.DoctorId,
                slot.SlotDate,
                StartTime = DateTime.Today.Add(slot.StartTime).ToString("hh:mm tt", CultureInfo.InvariantCulture),
                EndTime = DateTime.Today.Add(slot.EndTime).ToString("hh:mm tt", CultureInfo.InvariantCulture),
                slot.IsBooked
            });

            return Ok(response);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateSlot([FromBody] TimeSlot slot)
        {
            if (slot.DoctorId <= 0)
                return BadRequest("Invalid doctor.");

            if (slot.SlotDate.Date < DateTime.Today)
                return BadRequest("Slot date cannot be in the past.");

            if (slot.StartTime >= slot.EndTime)
                return BadRequest("Start time must be before end time.");

            bool isOneHour = slot.EndTime - slot.StartTime == TimeSpan.FromHours(1);
            bool inMorningWindow = slot.StartTime >= new TimeSpan(10, 0, 0) && slot.EndTime <= new TimeSpan(12, 0, 0);
            bool inEveningWindow = slot.StartTime >= new TimeSpan(13, 0, 0) && slot.EndTime <= new TimeSpan(17, 0, 0);

            if (!isOneHour || (!inMorningWindow && !inEveningWindow))
                return BadRequest("Slots must be 1 hour and only between 10:00-12:00 or 13:00-17:00.");

            bool created = repo.CreateSlot(slot);

            if (!created)
                return BadRequest("Unable to create slot.");

            return Ok(new { message = "Slot created successfully" });
        }

        [HttpPost("create-standard")]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateStandardSlots([FromBody] TimeSlot slot)
        {
            if (slot.DoctorId <= 0)
                return BadRequest("Invalid doctor.");

            if (slot.SlotDate.Date < DateTime.Today)
                return BadRequest("Slot date cannot be in the past.");

            int created = repo.CreateStandardSlotsForDate(slot.DoctorId, slot.SlotDate.Date);

            return Ok(new
            {
                message = "Standard slots processed successfully",
                createdCount = created
            });
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