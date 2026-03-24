namespace Appointment.API.Services
{
    public class SmsService
    {
        public bool SendDoctorUnavailableMessage(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            var message = "Doctor is not available kindly postpone the schedule.";

            // Placeholder SMS implementation.
            // Integrate Twilio or another provider here in production.
            Console.WriteLine($"SMS to {phoneNumber}: {message}");

            return true;
        }
    }
}
