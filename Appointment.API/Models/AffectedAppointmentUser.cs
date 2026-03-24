namespace Appointment.API.Models
{
    public class AffectedAppointmentUser
    {
        public int AppointmentId { get; set; }

        public int UserId { get; set; }

        public string? Name { get; set; }

        public string? Phone { get; set; }
    }
}