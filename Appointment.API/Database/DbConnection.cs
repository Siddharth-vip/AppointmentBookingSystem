using Microsoft.Data.SqlClient;

namespace Appointment.API.Database
{
    public class DbConnection
    {
        private readonly string connectionString =
            "Server=localhost;Database=AppointmentDB;Trusted_Connection=True;TrustServerCertificate=True";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}