namespace Appointment.UI.Models
{
    public class DoctorDashboardViewModel
    {
        public List<TimeSlot> Slots { get; set; } = new List<TimeSlot>();

        public List<DoctorAppointmentDetail> Appointments { get; set; } = new List<DoctorAppointmentDetail>();
    }
}