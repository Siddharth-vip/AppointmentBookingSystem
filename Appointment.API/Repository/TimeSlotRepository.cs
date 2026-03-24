using Appointment.API.Database;
using Appointment.API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

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

        public int CreateStandardSlotsForDate(int doctorId, DateTime slotDate)
        {
            var oneHourSlots = new List<(TimeSpan Start, TimeSpan End)>
            {
                (new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0)),
                (new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0)),
                (new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0)),
                (new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0)),
                (new TimeSpan(15, 0, 0), new TimeSpan(16, 0, 0)),
                (new TimeSpan(16, 0, 0), new TimeSpan(17, 0, 0))
            };

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string existingQuery = @"SELECT StartTime
                                         FROM TimeSlots
                                         WHERE DoctorId = @DoctorId
                                         AND SlotDate = @SlotDate";

                SqlCommand existingCmd = new SqlCommand(existingQuery, conn);
                existingCmd.Parameters.AddWithValue("@DoctorId", doctorId);
                existingCmd.Parameters.AddWithValue("@SlotDate", slotDate.Date);

                HashSet<TimeSpan> existingStarts = new HashSet<TimeSpan>();

                using (var reader = existingCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingStarts.Add((TimeSpan)reader["StartTime"]);
                    }
                }

                string insertQuery = @"INSERT INTO TimeSlots (DoctorId, SlotDate, StartTime, EndTime, IsBooked)
                                       VALUES (@DoctorId, @SlotDate, @StartTime, @EndTime, 0)";

                int created = 0;

                foreach (var slot in oneHourSlots)
                {
                    if (existingStarts.Contains(slot.Start))
                    {
                        continue;
                    }

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    insertCmd.Parameters.Add("@SlotDate", SqlDbType.Date).Value = slotDate.Date;
                    insertCmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = slot.Start;
                    insertCmd.Parameters.Add("@EndTime", SqlDbType.Time).Value = slot.End;

                    created += insertCmd.ExecuteNonQuery();
                }

                return created;
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