using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Service.Discord.UserGuild;
using Volvox.Helios.Service.Extensions;

namespace Volvox.Helios.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
                return View();

            return RedirectToAction("Index", "Dashboard");
        }
        
        public IActionResult Privacy() => View();

        public IActionResult About() => View();
    }
}