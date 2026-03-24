namespace Appointment.API.Services
{
    public class SmsService
    {
        public bool SendDoctorUnavailableMessage(string? phoneNumber, string? customMessage = null)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            var message = string.IsNullOrWhiteSpace(customMessage)
                ? "Doctor is not available kindly postpone the schedule."
                : customMessage;

            // Placeholder SMS implementation.
            // Integrate Twilio or another provider here in production.
            Console.WriteLine($"SMS to {phoneNumber}: {message}");

            return true;
        }
    }
}
