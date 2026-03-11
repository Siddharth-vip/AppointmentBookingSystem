using Microsoft.AspNetCore.Mvc;

namespace Appointment.UI.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var user = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}