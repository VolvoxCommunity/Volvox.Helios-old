using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Service.Discord.User;

namespace Volvox.Helios.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index([FromServices] IDiscordUserService userService)
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}