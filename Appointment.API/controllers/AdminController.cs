using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Appointment.API.Repository;
using Appointment.API.Models;

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

        [Authorize(Roles = "Admin")]
        [HttpPost("create-standard-slots")]
        public IActionResult CreateStandardSlots([FromBody] TimeSlot slot)
        {
            if (slot.DoctorId <= 0)
            {
                return BadRequest("Invalid doctor.");
            }

            if (slot.SlotDate.Date < DateTime.Today)
            {
                return BadRequest("Slot date cannot be in the past.");
            }

            int created = slotRepo.CreateStandardSlotsForDate(slot.DoctorId, slot.SlotDate.Date);

            return Ok(new
            {
                message = "Slots generated for 10:00-12:00 and 13:00-17:00 windows.",
                createdCount = created
            });
        }
    }
}