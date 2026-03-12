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

        [HttpPost("login")]
        public IActionResult Login(string email, string password)
        {
            var user = repo.Login(email, password);

            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            var token = jwt.GenerateToken(user.Email);

            return Ok(new
            {
                message = "Login successful",
                token = token,
                user = user
            });
        }
    }
}