using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Appointment.API.Repository;
using Appointment.API.Models;
using Appointment.API.Services;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        DoctorRepository doctorRepo = new DoctorRepository();
        TimeSlotRepository slotRepo = new TimeSlotRepository();
        AppointmentRepository appointmentRepo = new AppointmentRepository();
        SmsService smsService = new SmsService();

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

        [Authorize(Roles = "Admin")]
        [HttpPost("notify-slot-unavailable")]
        public IActionResult NotifySlotUnavailable([FromBody] SlotUnavailableRequest request)
        {
            if (request.DoctorId <= 0)
            {
                return BadRequest(new { message = "DoctorId is required." });
            }

            var slot = slotRepo.GetSlotByDateAndStartTime(request.DoctorId, request.SlotDate.Date, request.StartTime);
            if (slot == null)
            {
                return NotFound(new { message = "Slot not found for this doctor at selected date and time." });
            }

            var affectedUsers = appointmentRepo.GetBookedUsersBySlot(request.DoctorId, slot.SlotId);

            if (affectedUsers.Count == 0)
            {
                return Ok(new
                {
                    message = "No booked users found for this slot.",
                    notifiedCount = 0,
                    createdCount = 0
                });
            }

            TimeSlot? nextSlot = slotRepo.GetNextAvailableSlot(request.DoctorId, slot.SlotDate.Date, slot.StartTime);
            int createdCount = 0;

            for (int i = 1; i <= 7 && nextSlot == null; i++)
            {
                createdCount += slotRepo.CreateStandardSlotsForDate(request.DoctorId, slot.SlotDate.Date.AddDays(i));
                nextSlot = slotRepo.GetNextAvailableSlot(request.DoctorId, slot.SlotDate.Date, slot.StartTime);
            }

            int notifiedCount = 0;

            foreach (var user in affectedUsers)
            {
                string smsMessage;

                if (nextSlot != null)
                {
                    string start = DateTime.Today.Add(nextSlot.StartTime).ToString("hh:mm tt");
                    string end = DateTime.Today.Add(nextSlot.EndTime).ToString("hh:mm tt");
                    string currentStart = DateTime.Today.Add(slot.StartTime).ToString("hh:mm tt");

                    smsMessage = $"Dear {user.Name ?? "Patient"}, your doctor is unavailable on {slot.SlotDate:dd MMM yyyy} at {currentStart}. " +
                                 $"Please book next available slot on {nextSlot.SlotDate:dd MMM yyyy}, {start} - {end}.";
                }
                else
                {
                    string currentStart = DateTime.Today.Add(slot.StartTime).ToString("hh:mm tt");

                    smsMessage = $"Dear {user.Name ?? "Patient"}, your doctor is unavailable on {slot.SlotDate:dd MMM yyyy} at {currentStart}. " +
                                 "Please try again soon, new slots will be opened shortly.";
                }

                bool sent = smsService.SendDoctorUnavailableMessage(user.Phone, smsMessage);
                if (sent)
                {
                    notifiedCount++;
                }

                appointmentRepo.UpdateAppointmentStatus(
                    user.AppointmentId,
                    nextSlot == null ? "Doctor Unavailable" : "Reschedule Suggested");
            }

            string responseMessage = nextSlot == null
                ? "Users notified. No next slot found yet."
                : "Users notified with next available slot suggestion.";

            return Ok(new
            {
                message = responseMessage,
                notifiedCount,
                createdCount,
                suggestedSlot = nextSlot == null ? null : new
                {
                    nextSlot.SlotId,
                    nextSlot.SlotDate,
                    StartTime = DateTime.Today.Add(nextSlot.StartTime).ToString("hh:mm tt"),
                    EndTime = DateTime.Today.Add(nextSlot.EndTime).ToString("hh:mm tt")
                }
            });
        }
    }
}