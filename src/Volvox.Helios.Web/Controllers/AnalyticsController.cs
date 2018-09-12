using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Web.Filters;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/Analytics/{guildId}")]
    [IsUserGuildAdminFilter]
    public class AnalyticsController : Controller
    {
        public IActionResult Index(ulong guildId)
        {
            return View();
        }
    }
}