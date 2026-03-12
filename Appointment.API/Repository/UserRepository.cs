using System;
using Microsoft.Data.SqlClient;
using Appointment.API.Models;

namespace Appointment.API.Repository
{
    public class UserRepository
    {
        private readonly Appointment.API.Database.DbConnection db = new Appointment.API.Database.DbConnection();

        // REGISTER USER
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
                cmd.Parameters.AddWithValue("@Phone", user.Phone);
                cmd.Parameters.AddWithValue("@Role", user.Role);

                int rows = cmd.ExecuteNonQuery();

                return rows > 0;
            }
        }

        // LOGIN USER
        public User Login(string email, string password)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"SELECT TOP 1 * FROM Users 
                                 WHERE Email=@Email AND Password=@Password";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Role = reader["Role"].ToString()
                    };
                }

                return null;
            }
        }
    }
}