using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Domain.Discord;
using Volvox.Helios.Service.Discord;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Web.ViewModels.Settings;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        // TODO: Return all of the modules
        public IActionResult Index()
        {
            return View();
        }

        // GET
        public async Task<IActionResult> StreamAnnouncerSettings([FromServices] IDiscordUserService userService, [FromServices] IBot bot)
        {
            var guilds = await userService.GetUserGuilds();
            
            var botGuilds = bot.GetGuilds();

            var viewModel = new StreamAnnouncerSettingsViewModel()
            {
                Guilds = new SelectList(
                    guilds.FilterAdministrator().FilterGuildsByIds(botGuilds.Select(b => b.Id).ToList()), "Id", "Name")
            };
            
            return View(viewModel);
        }
        
        // POST
        [HttpPost]
        public async Task<IActionResult> StreamAnnouncerSettings(StreamAnnouncerSettingsViewModel viewModel)
        {
            return Index();
        }

        // GET
        [HttpGet]
        public async Task<JsonResult> GetGuildChannels([FromServices] IDiscordGuildService guildService, ulong guildId)
        {
            var channels = await guildService.GetChannels(guildId);

            return Json(channels.FilterChannelType(0));
        }
    }
}