namespace Appointment.UI.Models
{
    public class UserSlotsDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;

        public List<UserAppointmentStatusItem> BookedSlots { get; set; } = new List<UserAppointmentStatusItem>();

        public List<UserAppointmentStatusItem> UnavailableSlots { get; set; } = new List<UserAppointmentStatusItem>();
    }
}