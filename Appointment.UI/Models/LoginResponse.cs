namespace Appointment.UI.Models
{
    public class LoginResponse
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public User User { get; set; }
    }
}