using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
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

        public IActionResult Index(ulong guildId, [FromServices] IBot bot,
            [FromServices] IDiscordSettings discordSettings)
        {
            if (bot.IsBotInGuild(guildId))
                return View(guildId);

            // Bot is not in guild so redirect to the add bot URL.
            var redirectUrl = Uri.EscapeDataString($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}");

            return Redirect(
                $"https://discordapp.com/api/oauth2/authorize?client_id={discordSettings.ClientId}&permissions=8&redirect_uri={redirectUrl}&scope=bot&guild_id={guildId}");

        }

        #region StreamAnnouncer
        // GET
        [HttpGet("StreamAnnouncer")]
        public async Task<IActionResult> StreamAnnouncerSettings(ulong guildId,
            [FromServices] IDiscordGuildService guildService)
        {
            var channels = await guildService.GetChannels(guildId);

            var viewModel = new StreamAnnouncerSettingsViewModel
            {
                Channels = new SelectList(channels.FilterChannelType(0), "Id", "Name")
            };

            var settings = await _streamAnnouncerSettingsService.GetSettingsByGuild(guildId);

            if (settings == null) return View(viewModel);

            viewModel.ChannelId = settings.AnnouncementChannelId;
            viewModel.Enabled = settings.Enabled;

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