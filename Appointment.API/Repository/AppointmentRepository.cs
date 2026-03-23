using Appointment.API.Database;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Appointment.API.Models;

namespace Appointment.API.Repository
{
    public class AppointmentRepository
    {
        DbConnection db = new DbConnection();

        // ===============================
        // BOOK APPOINTMENT
        // ===============================
        public void BookAppointment(Appointment.API.Models.Appointment appointment)
{
    using (SqlConnection conn = db.GetConnection())
    {
        conn.Open();

        Console.WriteLine("🔥 DB Connected: " + conn.Database);
        Console.WriteLine("🔥 SlotId: " + appointment.SlotId);

        SqlTransaction transaction = conn.BeginTransaction();

        try
        {
            // 🔒 1. Check slot
            string checkQuery = @"SELECT IsBooked FROM TimeSlots WHERE SlotId = @SlotId";

            SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction);
            checkCmd.Parameters.AddWithValue("@SlotId", appointment.SlotId);

            var result = checkCmd.ExecuteScalar();

            if (result == null)
            {
                Console.WriteLine("❌ Slot not found");
                transaction.Rollback();
                return;
            }

            // ⚠️ DO NOT THROW — just stop
            if (Convert.ToInt32(result) == 1)
            {
                Console.WriteLine("⚠ Slot already booked");
                transaction.Rollback();
                return;
            }

            // ✅ 2. Insert appointment
            string insertQuery = @"INSERT INTO Appointments
                (UserId, DoctorId, SlotId, AppointmentDate, Status)
                VALUES
                (@UserId, @DoctorId, @SlotId, @AppointmentDate, @Status)";

            SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction);

            insertCmd.Parameters.AddWithValue("@UserId", appointment.UserId);
            insertCmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
            insertCmd.Parameters.AddWithValue("@SlotId", appointment.SlotId);
            insertCmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
            insertCmd.Parameters.AddWithValue("@Status", appointment.Status ?? "Booked");

            insertCmd.ExecuteNonQuery();

            Console.WriteLine("✅ Insert Success");

            // 🔥 3. Update slot
            string updateQuery = @"UPDATE TimeSlots
                                   SET IsBooked = 1
                                   WHERE SlotId = @SlotId";

            SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
            updateCmd.Parameters.AddWithValue("@SlotId", appointment.SlotId);

            int rows = updateCmd.ExecuteNonQuery();

            Console.WriteLine("🔥 Rows Updated: " + rows);

            if (rows == 0)
            {
                Console.WriteLine("⚠ No rows updated");
                transaction.Rollback();
                return;
            }

            // ✅ 4. Commit
            transaction.Commit();

            Console.WriteLine("🎉 Booking successful + Slot updated");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine("❌ ERROR: " + ex.Message);
            throw;
        }
    }
}

        // ===============================
        // GET ALL APPOINTMENTS
        // ===============================
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
                        AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                        Status = reader["Status"].ToString()
                    });
                }
            }

            return list;
        }

        // ===============================
        // USER APPOINTMENTS
        // ===============================
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
                        AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                        Status = reader["Status"].ToString()
                    });
                }
            }

            return list;
        }

        // ===============================
        // DOCTOR APPOINTMENTS
        // ===============================
        public List<dynamic> GetAppointmentsByDoctor(int doctorId)
        {
            List<dynamic> list = new List<dynamic>();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"SELECT a.AppointmentId,
                                        a.UserId,
                                        a.DoctorId,
                                        a.SlotId,
                                        a.AppointmentDate,
                                        a.Status,
                                        u.Name AS PatientName,
                                        u.Email AS PatientEmail,
                                        t.SlotDate,
                                        t.StartTime,
                                        t.EndTime
                                 FROM Appointments a
                                 INNER JOIN Users u ON a.UserId = u.UserId
                                 INNER JOIN TimeSlots t ON a.SlotId = t.SlotId
                                 WHERE a.DoctorId = @DoctorId
                                 ORDER BY t.SlotDate, t.StartTime";

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
                        AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                        Status = reader["Status"].ToString(),
                        PatientName = reader["PatientName"].ToString(),
                        PatientEmail = reader["PatientEmail"].ToString(),
                        SlotDate = Convert.ToDateTime(reader["SlotDate"]),
                        StartTime = reader["StartTime"].ToString(),
                        EndTime = reader["EndTime"].ToString()
                    });
                }
            }

            return list;
        }
    }
}