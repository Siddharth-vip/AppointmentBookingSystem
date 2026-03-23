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

        public bool CreateSlot(TimeSlot slot)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"INSERT INTO TimeSlots (DoctorId, SlotDate, StartTime, EndTime, IsBooked)
                                 VALUES (@DoctorId, @SlotDate, @StartTime, @EndTime, 0)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DoctorId", slot.DoctorId);
                cmd.Parameters.AddWithValue("@SlotDate", slot.SlotDate.Date);
                cmd.Parameters.AddWithValue("@StartTime", slot.StartTime);
                cmd.Parameters.AddWithValue("@EndTime", slot.EndTime);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteSlot(int slotId, int doctorId)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"DELETE FROM TimeSlots
                                 WHERE SlotId = @SlotId
                                 AND DoctorId = @DoctorId
                                 AND IsBooked = 0";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SlotId", slotId);
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}