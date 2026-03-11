using Microsoft.AspNetCore.Mvc;
using Appointment.API.Repository;
using Appointment.API.Models;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly DoctorRepository repo;

        public DoctorController()
        {
            repo = new DoctorRepository();
        }

        // GET: api/doctor/all
        [HttpGet("all")]
        public IActionResult GetAllDoctors()
        {
            var doctors = repo.GetAllDoctors();

            if (doctors == null || doctors.Count == 0)
            {
                return NotFound("No doctors found");
            }

            return Ok(doctors);
        }
    }
}