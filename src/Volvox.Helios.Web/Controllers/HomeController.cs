using Microsoft.AspNetCore.Mvc;

namespace Volvox.Helios.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Privacy() => View();

        public IActionResult About() => View();

        public IActionResult Example() => View();

        public IActionResult Example2() => View();
    }
}