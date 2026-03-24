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
        public AppointmentBookingResult BookAppointment(Appointment.API.Models.Appointment appointment)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string checkQuery = @"SELECT IsBooked
                                          FROM TimeSlots
                                          WHERE SlotId = @SlotId
                                          AND DoctorId = @DoctorId";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction);
                    checkCmd.Parameters.AddWithValue("@SlotId", appointment.SlotId);
                    checkCmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);

                    var result = checkCmd.ExecuteScalar();

                    if (result == null)
                    {
                        transaction.Rollback();

                        return new AppointmentBookingResult
                        {
                            Success = false,
                            ErrorCode = "SLOT_UNAVAILABLE",
                            Message = "Doctor is not available kindly postpone the schedule."
                        };
                    }

                    if (Convert.ToInt32(result) == 1)
                    {
                        transaction.Rollback();

                        return new AppointmentBookingResult
                        {
                            Success = false,
                            ErrorCode = "SLOT_UNAVAILABLE",
                            Message = "Doctor is not available kindly postpone the schedule."
                        };
                    }

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

                    string updateQuery = @"UPDATE TimeSlots
                                           SET IsBooked = 1
                                           WHERE SlotId = @SlotId";

                    SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
                    updateCmd.Parameters.AddWithValue("@SlotId", appointment.SlotId);

                    int rows = updateCmd.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        transaction.Rollback();

                        return new AppointmentBookingResult
                        {
                            Success = false,
                            ErrorCode = "BOOKING_FAILED",
                            Message = "Unable to book appointment. Please try again."
                        };
                    }

                    transaction.Commit();

                    return new AppointmentBookingResult
                    {
                        Success = true,
                        Message = "Appointment booked successfully"
                    };
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public string? GetUserPhone(int userId)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT TOP 1 Phone FROM Users WHERE UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? null : result.ToString();
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
                    var startTime = (TimeSpan)reader["StartTime"];
                    var endTime = (TimeSpan)reader["EndTime"];

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
                        StartTime = DateTime.Today.Add(startTime).ToString("hh:mm tt"),
                        EndTime = DateTime.Today.Add(endTime).ToString("hh:mm tt")
                    });
                }
            }

            return list;
        }
    }
}