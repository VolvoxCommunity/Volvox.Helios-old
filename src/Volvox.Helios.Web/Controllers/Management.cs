using Microsoft.AspNetCore.Mvc;

namespace Volvox.Helios.Web.Controllers
{
    public class ManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StreamRole() => View();

        public IActionResult StreamAnnouncer() => View();

        public IActionResult ChatTracker() => View();

        public IActionResult Remembot() => View();

        public IActionResult PollManager() => View();
    }
}