using Microsoft.AspNetCore.Mvc;
using Appointment.API.Repository;
using Appointment.API.Services;
using Appointment.API.Models;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        UserRepository repo = new UserRepository();
        JwtService jwt = new JwtService();

        // ===============================
        // LOGIN USER
        // ===============================
        [HttpPost("login")]
        public IActionResult Login([FromQuery] string email, [FromQuery] string password)
        {
            Console.WriteLine("🔥 LOGIN HIT");
            Console.WriteLine("Email: " + email);

            var user = repo.Login(email, password); // ✅ FIXED

            if (user == null)
            {
                return BadRequest("Invalid email or password");
            }

            var token = jwt.GenerateToken(user.Email ?? string.Empty, user.Role);

            return Ok(new
{
    message = "Login successful",
    token = token,
    role = user.Role,   // ✅ ADD THIS
    user = user
});
        }

        // ===============================
        // REGISTER USER (NEW)
        // ===============================
        [HttpPost("register")]
        public IActionResult Register([FromBody] Appointment.API.Models.User user)
        {
            Console.WriteLine("🔥 REGISTER HIT");
            Console.WriteLine("Email: " + user.Email);

            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Invalid data");
            }

            var existingUser = repo.GetUserByEmail(user.Email);

            if (existingUser != null)
            {
                return BadRequest("User already exists");
            }

            bool result = repo.RegisterUser(user);

            if (!result)
            {
                return BadRequest("Registration failed");
            }

            return Ok(new
            {
                message = "User registered successfully"
            });
        }
    }
}