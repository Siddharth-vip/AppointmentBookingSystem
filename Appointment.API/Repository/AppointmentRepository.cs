using Appointment.API.Database;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Appointment.API.Repository
{
    public class AppointmentRepository
    {
        DbConnection db = new DbConnection();

        // ============================
        // BOOK APPOINTMENT
        // ============================
        public void BookAppointment(int userId, int doctorId, int slotId, DateTime appointmentDate)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"INSERT INTO Appointments(UserId, DoctorId, SlotId, AppointmentDate)
                                 VALUES(@UserId, @DoctorId, @SlotId, @AppointmentDate)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                cmd.Parameters.AddWithValue("@SlotId", slotId);
                cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate);

                cmd.ExecuteNonQuery();

                // mark slot as booked
                string updateSlot = "UPDATE TimeSlots SET IsBooked = 1 WHERE SlotId = @SlotId";

                SqlCommand slotCmd = new SqlCommand(updateSlot, conn);
                slotCmd.Parameters.AddWithValue("@SlotId", slotId);

                slotCmd.ExecuteNonQuery();
            }
        }


        // ============================
        // GET ALL APPOINTMENTS
        // ============================
        public List<dynamic> GetAllAppointments()
        {
            List<dynamic> list = new List<dynamic>();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM Appointments";

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        DoctorId = Convert.ToInt32(reader["DoctorId"]),
                        SlotId = Convert.ToInt32(reader["SlotId"]),
                        AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"])
                    });
                }
            }

            return list;
        }


        // ============================
        // USER APPOINTMENTS
        // ============================
        public List<dynamic> GetAppointmentsByUser(int userId)
        {
            List<dynamic> list = new List<dynamic>();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM Appointments WHERE UserId = @UserId";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                        DoctorId = Convert.ToInt32(reader["DoctorId"]),
                        SlotId = Convert.ToInt32(reader["SlotId"]),
                        AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"])
                    });
                }
            }

            return list;
        }


        // ============================
        // DOCTOR APPOINTMENTS
        // ============================
        public List<dynamic> GetAppointmentsByDoctor(int doctorId)
        {
            List<dynamic> list = new List<dynamic>();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM Appointments WHERE DoctorId = @DoctorId";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@DoctorId", doctorId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        SlotId = Convert.ToInt32(reader["SlotId"]),
                        AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"])
                    });
                }
            }

            return list;
        }
    }
}