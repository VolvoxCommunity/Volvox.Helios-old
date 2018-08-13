using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
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
            [FromServices] IDiscordGuildService guildService)
        {
            var settings = await _streamAnnouncerSettingsService.GetSettingsByGuild(guildId);

            var viewModel = new StreamAnnouncerSettingsViewModel
            {
                ChannelId = settings.AnnouncementChannelId,
                Enabled = settings.Enabled,
                Channels = new SelectList(await guildService.GetChannels(guildId), "Id", "Name")
            };

            return View(viewModel);
        }

        // POST
        [HttpPost("StreamAnnouncer")]
        public async Task<IActionResult> StreamAnnouncerSettings(ulong guildId, StreamAnnouncerSettingsViewModel viewModel)
        {
            // Save the settings to the database
            await _streamAnnouncerSettingsService.SaveSettings(new StreamAnnouncerSettings
            {
                GuildId = guildId,
                Enabled = viewModel.Enabled,
                AnnouncementChannelId = viewModel.ChannelId
            });

            return RedirectToAction("Index");
        }

        #endregion
    }
}