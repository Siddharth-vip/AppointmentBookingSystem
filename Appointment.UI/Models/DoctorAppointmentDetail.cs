namespace Appointment.UI.Models
{
    public class DoctorAppointmentDetail
    {
        public int AppointmentId { get; set; }

        public int UserId { get; set; }

        public int SlotId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string? Status { get; set; }

        public string? PatientName { get; set; }

        public string? PatientEmail { get; set; }

        public string? PatientPhone { get; set; }

        public DateTime SlotDate { get; set; }

        public string? StartTime { get; set; }

        public string? EndTime { get; set; }
    }
}