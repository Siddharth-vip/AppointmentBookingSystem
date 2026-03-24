namespace Appointment.UI.Models
{
    public class BookingApiResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool SmsSent { get; set; }
    }
}
