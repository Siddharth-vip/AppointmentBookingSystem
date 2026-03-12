using Appointment.API.Database;
using Appointment.API.Models;
using Microsoft.Data.SqlClient;

namespace Appointment.API.Repository
{
    public class TimeSlotRepository
    {
        private readonly DbConnection db = new DbConnection();

        // GET ALL SLOTS FOR DOCTOR
        public List<TimeSlot> GetSlotsByDoctor(int doctorId)
        {
            List<TimeSlot> slots = new List<TimeSlot>();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"SELECT SlotId, DoctorId, SlotDate, StartTime, EndTime, IsBooked
                                 FROM TimeSlots
                                 WHERE DoctorId = @DoctorId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    slots.Add(new TimeSlot
                    {
                        SlotId = Convert.ToInt32(reader["SlotId"]),
                        DoctorId = Convert.ToInt32(reader["DoctorId"]),

                        SlotDate = reader["SlotDate"] == DBNull.Value
                            ? DateTime.Today
                            : Convert.ToDateTime(reader["SlotDate"]),

                        StartTime = (TimeSpan)reader["StartTime"],
                        EndTime = (TimeSpan)reader["EndTime"],

                        IsBooked = Convert.ToBoolean(reader["IsBooked"])
                    });
                }
            }

            return slots;
        }

        // USED BY CONTROLLER
        public List<TimeSlot> GetAvailableSlots(int doctorId)
        {
            return GetSlotsByDoctor(doctorId);
        }

        // ADMIN DASHBOARD
        public int GetTotalAppointments()
        {
            int total = 0;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM TimeSlots WHERE IsBooked = 1";

                SqlCommand cmd = new SqlCommand(query, conn);

                total = (int)cmd.ExecuteScalar();
            }

            return total;
        }
    }
}