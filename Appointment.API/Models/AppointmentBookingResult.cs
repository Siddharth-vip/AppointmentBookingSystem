namespace Appointment.API.Models
{
    public class AppointmentBookingResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? ErrorCode { get; set; }
    }
}
