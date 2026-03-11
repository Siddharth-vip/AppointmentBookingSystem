using Microsoft.Data.SqlClient;
using Appointment.API.Database;
using Appointment.API.Models;

namespace Appointment.API.Repository
{
    public class DoctorRepository
    {
        private readonly DbConnection db = new DbConnection();

        // GET ALL DOCTORS
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
                    Doctor doctor = new Doctor
                    {
                        DoctorId = Convert.ToInt32(reader["DoctorId"]),
                        DoctorName = reader["DoctorName"].ToString(),
                        Specialization = reader["Specialization"].ToString(),
                        Experience = Convert.ToInt32(reader["Experience"]),
                        Phone = reader["Phone"].ToString()
                    };

                    doctors.Add(doctor);
                }
            }

            return doctors;
        }

        // GET TOTAL DOCTORS (Used by AdminController)
        public int GetTotalDoctors()
        {
            int count = 0;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM Doctors";

                SqlCommand cmd = new SqlCommand(query, conn);

                count = (int)cmd.ExecuteScalar();
            }

            return count;
        }
    }
}