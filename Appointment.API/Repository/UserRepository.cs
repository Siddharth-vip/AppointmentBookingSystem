using Appointment.API.Database;
using Appointment.API.Models;
using Microsoft.Data.SqlClient;

namespace Appointment.API.Repository
{
    public class UserRepository
    {
        private readonly DbConnection db = new DbConnection();

        // LOGIN USER
        public User LoginUser(string email, string password)
        {
            User user = null;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT TOP 1 * FROM Users WHERE Email=@Email AND Password=@Password";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    user = new User
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Role = reader["Role"]?.ToString()
                    };
                }
            }

            return user;
        }


        // REGISTER USER
        public bool RegisterUser(User user)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"INSERT INTO Users (Name, Email, Password, Phone, CreatedDate, Role)
                                 VALUES (@Name, @Email, @Password, @Phone, GETDATE(), 'User')";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@Phone", user.Phone ?? "");

                int rows = cmd.ExecuteNonQuery();

                return rows > 0;
            }
        }


        // GET USER BY EMAIL
        public User GetUserByEmail(string email)
        {
            User user = null;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT TOP 1 * FROM Users WHERE Email=@Email";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Email", email);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    user = new User
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Role = reader["Role"]?.ToString()
                    };
                }
            }

            return user;
        }
    }
}