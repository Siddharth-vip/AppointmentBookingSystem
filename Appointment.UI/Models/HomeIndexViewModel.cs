namespace Appointment.UI.Models
{
    public class HomeIndexViewModel
    {
        public bool IsLoggedIn { get; set; }

        public bool IsAdmin { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int TotalBookedCount { get; set; }

        public int TotalUnavailableCount { get; set; }

        public bool ShowBookedJustNow { get; set; }

        public UserAppointmentStatusItem? BookedJustNow { get; set; }

        public List<UserAppointmentStatusItem> RecentBookedSlots { get; set; } = new List<UserAppointmentStatusItem>();

        public List<UserAppointmentStatusItem> RecentUnavailableSlots { get; set; } = new List<UserAppointmentStatusItem>();
    }

    public class UserAppointmentStatusItem
    {
        public int AppointmentId { get; set; }

        public int UserId { get; set; }

        public int DoctorId { get; set; }

        public int SlotId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string? Status { get; set; }

        public string? DoctorName { get; set; }

        public DateTime SlotDate { get; set; }

        public string? StartTime { get; set; }

        public string? EndTime { get; set; }
    }
}