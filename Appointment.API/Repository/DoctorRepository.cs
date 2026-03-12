using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Appointment.API.Database;
using Appointment.API.Models;

namespace Appointment.API.Repository
{
    public class DoctorRepository
    {
        DbConnection db = new DbConnection();

        // ======================================
        // GET ALL DOCTORS
        // ======================================
        public List<Doctor> GetAllDoctors()
        {
            List<Doctor> doctors = new List<Doctor>();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM Doctors";

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    doctors.Add(new Doctor
                    {
                        DoctorId = Convert.ToInt32(reader["DoctorId"]),
                        DoctorName = reader["DoctorName"].ToString(),
                        Specialization = reader["Specialization"].ToString(),
                        Experience = Convert.ToInt32(reader["Experience"]),
                        Phone = reader["Phone"].ToString()
                    });
                }
            }

            return doctors;
        }


        // ======================================
        // GET AVAILABLE DOCTORS
        // ======================================
        public List<Doctor> GetAvailableDoctors(DateTime date, TimeSpan time)
        {
            List<Doctor> doctors = new List<Doctor>();

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"
                SELECT DISTINCT d.*
                FROM Doctors d
                JOIN TimeSlots t ON d.DoctorId = t.DoctorId
                WHERE t.SlotDate = @date
                AND @time BETWEEN t.StartTime AND t.EndTime
                AND t.IsBooked = 0";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@time", time);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    doctors.Add(new Doctor
                    {
                        DoctorId = Convert.ToInt32(reader["DoctorId"]),
                        DoctorName = reader["DoctorName"].ToString(),
                        Specialization = reader["Specialization"].ToString(),
                        Experience = Convert.ToInt32(reader["Experience"]),
                        Phone = reader["Phone"].ToString()
                    });
                }
            }

            return doctors;
        }


        // ======================================
        // GET TOTAL DOCTORS (ADMIN DASHBOARD)
        // ======================================
        public int GetTotalDoctors()
        {
            int total = 0;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM Doctors";

                SqlCommand cmd = new SqlCommand(query, conn);

                total = (int)cmd.ExecuteScalar();
            }

            return total;
        }
    }
}