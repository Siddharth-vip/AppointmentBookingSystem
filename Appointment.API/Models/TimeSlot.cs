namespace Appointment.API.Models
{
    public class TimeSlot
    {
        public int SlotId { get; set; }

        public int DoctorId { get; set; }

        public DateTime SlotDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsBooked { get; set; }
    }
}