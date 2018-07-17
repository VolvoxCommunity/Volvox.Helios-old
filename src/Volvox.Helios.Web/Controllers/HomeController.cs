using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Service.Clients;
using Volvox.Helios.Service.Discord;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Web.Models;

namespace Volvox.Helios.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index([FromServices] IDiscordUserService userService)
        {
            return View();
        }

        public async Task<IActionResult> UserGuilds([FromServices] IDiscordUserService userService)
        {
            var guilds = await userService.GetUserGuilds();
            
            return View(guilds.FilterAdministrator());
        }
    }
}
