using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Service.Discord.UserGuild;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Web.ViewModels.Dashboard;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDiscordUserGuildService _userGuildService;
        private readonly IBot _bot;

        public DashboardController(IDiscordUserGuildService userGuildService, IBot bot)
        {
            _userGuildService = userGuildService;
            _bot = bot;
        }

        public async Task<IActionResult> Index()
        {
            var guilds = await _userGuildService.GetUserGuilds();

            // Populate guild details.
            foreach (var guild in guilds)
            {
                guild.Guild.Details.IsBotInGuild = _bot.IsBotInGuild(guild.Guild.Id);

                // Bot must be in the guild to retrieve details.
                if (guild.Guild.Details.IsBotInGuild)
                    guild.Guild.Details.MemberCount = _bot.GetGuild(guild.Guild.Id).MemberCount;
            }

            var viewModel = new DashboardIndexViewModel
            {
                UserGuilds = guilds.FilterAdministrator()
            };

            return View(viewModel);
        }

        [Route("Dashboard/{guildId}")]
        public async Task<IActionResult> Details(ulong guildId)
        {
            var guilds = await _userGuildService.GetUserGuilds();

            // Populate guild details.
            foreach (var guild in guilds)
            {
                guild.Guild.Details.IsBotInGuild = _bot.IsBotInGuild(guild.Guild.Id);

                // Bot must be in the guild to retrieve details.
                if (guild.Guild.Details.IsBotInGuild)
                    guild.Guild.Details.MemberCount = _bot.GetGuild(guild.Guild.Id).MemberCount;
            }

            var viewModel = new DashboardDetailsViewModel
            {
                UserGuilds = guilds.FilterAdministrator(),
                Guild = guilds.FirstOrDefault(g => g.Guild.Id == guildId)?.Guild
            };

            return View(viewModel);
        }
    }
}