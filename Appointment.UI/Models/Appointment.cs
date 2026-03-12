namespace Appointment.UI.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        public int UserId { get; set; }

        public int DoctorId { get; set; }

        public int SlotId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string? Status { get; set; }
    }
}