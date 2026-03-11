using Microsoft.AspNetCore.Mvc;
using Appointment.API.Repository;
using Appointment.API.Models;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository repo = new UserRepository();


        // LOGIN
        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            var loggedUser = repo.LoginUser(user.Email, user.Password);

            if (loggedUser == null)
                return Unauthorized("Invalid login");

            return Ok(loggedUser);
        }


        // REGISTER
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            var success = repo.RegisterUser(user);

            if (!success)
                return BadRequest("Registration failed");

            return Ok("User registered successfully");
        }


        // GET USER BY EMAIL
        [HttpGet("email/{email}")]
        public IActionResult GetUserByEmail(string email)
        {
            var user = repo.GetUserByEmail(email);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}