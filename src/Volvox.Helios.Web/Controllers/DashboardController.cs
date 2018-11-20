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

        public DashboardController(IDiscordUserGuildService userGuildService)
        {
            _userGuildService = userGuildService;
        }


        public async Task<IActionResult> Index([FromServices] IBot bot)
        {
            var guilds = await _userGuildService.GetUserGuilds();

            // Populate guild details.
            foreach (var guild in guilds)
            {
                guild.Guild.Details.IsBotInGuild = bot.IsBotInGuild(guild.Guild.Id);

                // Bot must be in the guild to retrieve details.
                if (guild.Guild.Details.IsBotInGuild)
                    guild.Guild.Details.MemberCount = bot.GetGuild(guild.Guild.Id).MemberCount;
            }

            var viewModel = new DashboardIndexViewModel
            {
                UserGuilds = guilds.FilterAdministrator()
            };

            return View(viewModel);
        }
    }
}