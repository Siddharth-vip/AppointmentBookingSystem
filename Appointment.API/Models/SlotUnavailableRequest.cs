namespace Appointment.API.Models
{
    public class SlotUnavailableRequest
    {
        public int DoctorId { get; set; }

        public DateTime SlotDate { get; set; }

        public TimeSpan StartTime { get; set; }
    }
}