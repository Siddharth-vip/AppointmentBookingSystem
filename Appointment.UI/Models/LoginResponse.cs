namespace Appointment.UI.Models
{
    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public User User { get; set; } = new User();
    }
}