using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.ViewModels.Settings;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/Settings/{guildId}")]
    public class SettingsController : Controller
    {
        private readonly IModuleSettingsService<StreamAnnouncerSettings> _streamAnnouncerSettingsService;

        public SettingsController(IModuleSettingsService<StreamAnnouncerSettings> streamAnnouncerSettingsService)
        {
            _streamAnnouncerSettingsService = streamAnnouncerSettingsService;
        }

        public IActionResult Index(ulong guildId)
        {
            return View(guildId);
        }

        #region StreamAnnouncer

        // GET
        [HttpGet("StreamAnnouncer")]
        public async Task<IActionResult> StreamAnnouncerSettings(ulong guildId,
            [FromServices] IDiscordUserService userService,
            [FromServices] IBot bot, [FromServices] IModuleSettingsService<StreamAnnouncerSettings> settingsService)
        {
            var guilds = await userService.GetUserGuilds();

            var botGuilds = bot.GetGuilds();

            var settings = await settingsService.GetSettingsByGuild(guildId);

            var viewModel = new StreamAnnouncerSettingsViewModel
            {
                ChannelId = settings.AnnouncementChannelId,
                Enabled = settings.Enabled,
                Guilds = new SelectList(
                    guilds.FilterAdministrator().FilterGuildsByIds(botGuilds.Select(b => b.Id).ToList()), "Id", "Name")
            };

            return View(viewModel);
        }

        // POST
        [HttpPost("StreamAnnouncer")]
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