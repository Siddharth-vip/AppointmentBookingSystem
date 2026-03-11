namespace Appointment.UI.Models
{
    public class TimeSlot
    {
        public int SlotId { get; set; }

        public int DoctorId { get; set; }

        public DateTime SlotDate { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public bool IsBooked { get; set; }
    }
}