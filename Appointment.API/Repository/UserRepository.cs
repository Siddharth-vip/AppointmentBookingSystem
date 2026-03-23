using System;
using Microsoft.Data.SqlClient;
using Appointment.API.Models;

namespace Appointment.API.Repository
{
    public class UserRepository
    {
        private readonly Appointment.API.Database.DbConnection db = new Appointment.API.Database.DbConnection();

        // ===============================
        // REGISTER USER
        // ===============================
        public bool RegisterUser(User user)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"INSERT INTO Users 
                                (Name, Email, Password, Phone, CreatedDate, Role)
                                VALUES 
                                (@Name, @Email, @Password, @Phone, GETDATE(), @Role)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@Phone", user.Phone ?? "");

                cmd.Parameters.AddWithValue("@Role", string.IsNullOrEmpty(user.Role) ? "User" : user.Role);

                int rows = cmd.ExecuteNonQuery();

                return rows > 0;
            }
        }

        // ===============================
        // LOGIN USER (FIXED)
        // ===============================
        public User? Login(string email, string password)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                Console.WriteLine("🔥 Connected DB: " + conn.Database);
                Console.WriteLine("Email Input: " + email);
                Console.WriteLine("Password Input: " + password);

                string query = @"SELECT TOP 1 * FROM Users 
                                 WHERE Email = @Email AND Password = @Password";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Email", email.Trim());
                cmd.Parameters.AddWithValue("@Password", password.Trim());

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Console.WriteLine("✅ USER FOUND");

                    return new User
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Name = reader["Name"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Role = reader["Role"]?.ToString()
                    };
                }

                Console.WriteLine("❌ USER NOT FOUND");

                return null;
            }
        }

        // ===============================
        // GET USER BY EMAIL (ADDED)
        // ===============================
        public User? GetUserByEmail(string email)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT TOP 1 * FROM Users WHERE Email = @Email";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email.Trim());

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Name = reader["Name"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Role = reader["Role"]?.ToString()
                    };
                }

                return null;
            }
        }
    }
}