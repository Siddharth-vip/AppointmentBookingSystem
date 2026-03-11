using Microsoft.AspNetCore.Mvc;

namespace Appointment.UI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}