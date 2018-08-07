using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.ViewModels.Settings;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/Settings")]
    public class SettingsController : Controller
    {
        private readonly IModuleSettingsService<StreamAnnouncerSettings> _streamAnnouncerSettingsService;

        public SettingsController(IModuleSettingsService<StreamAnnouncerSettings> streamAnnouncerSettingsService)
        {
            _streamAnnouncerSettingsService = streamAnnouncerSettingsService;
        }

        [Route("{guildId}")]
        public IActionResult Index(ulong guildId)
        {
            return View(guildId);
        }

        // GET
        [HttpGet("GetGuildChannels")]
        public async Task<JsonResult> GetGuildChannels([FromServices] IDiscordGuildService guildService, ulong guildId)
        {
            var channels = await guildService.GetChannels(guildId);

            // Format the ulong to string.
            return Json(channels.FilterChannelType(0).Select(c => new {id = c.Id.ToString(), name = c.Name}));
        }
        
        [HttpGet("GetUserAdminGuilds")]
        public async Task<JsonResult> GetUserAdminGuilds([FromServices] IDiscordUserService userService)
        {
            var guilds = await userService.GetUserGuilds();

            // Format the ulong to string.
            return Json(guilds.FilterAdministrator());
        }

        #region StreamAnnouncer

        // GET
        [HttpGet("{guildId}/StreamAnnouncer")]
        public async Task<IActionResult> StreamAnnouncerSettings(ulong guildId, [FromServices] IDiscordUserService userService,
            [FromServices] IBot bot)
        {
            var guilds = await userService.GetUserGuilds();

            var botGuilds = bot.GetGuilds();

            var viewModel = new StreamAnnouncerSettingsViewModel
            {
                Guilds = new SelectList(
                    guilds.FilterAdministrator().FilterGuildsByIds(botGuilds.Select(b => b.Id).ToList()), "Id", "Name")
            };

            return View(viewModel);
        }

        // POST
        [HttpPost("{guildId}/StreamAnnouncer")]
        public async Task<IActionResult> StreamAnnouncerSettings(StreamAnnouncerSettingsViewModel viewModel)
        {
            // Save the settings to the database
            await _streamAnnouncerSettingsService.SaveSettings(new StreamAnnouncerSettings
            {
                GuildId = viewModel.GuildId,
                Enabled = viewModel.Enabled,
                AnnouncementChannelId = viewModel.ChannelId
            });

            return RedirectToAction("Index");
        }

        #endregion
    }
}