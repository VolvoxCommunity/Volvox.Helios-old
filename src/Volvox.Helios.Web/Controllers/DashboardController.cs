using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Service.Discord.UserGuild;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Web.ViewModels.Dashboard;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDiscordUserGuildService _userGuildService;

        public DashboardController(IDiscordUserGuildService userGuildService)
        {
            _userGuildService = userGuildService;
        }


        public async Task<IActionResult> Index()
        {
            var guilds = await _userGuildService.GetUserGuilds();

            var viewModel = new DashboardIndexViewModel
            {
                UserGuilds = guilds.FilterAdministrator()
            };

            return View(viewModel);
        }
    }
}