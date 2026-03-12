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

        // ==========================================
        // GET ALL DOCTORS
        // api/doctor/all
        // ==========================================
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


        // ==========================================
        // GET AVAILABLE DOCTORS
        // api/doctor/available?date=2026-03-06&time=10:00:00
        // ==========================================
        [HttpGet("available")]
        public IActionResult GetAvailableDoctors(DateTime date, TimeSpan time)
        {
            try
            {
                var doctors = repo.GetAvailableDoctors(date, time);

                if (doctors == null || doctors.Count == 0)
                {
                    return NotFound("No doctors available at this time");
                }

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}