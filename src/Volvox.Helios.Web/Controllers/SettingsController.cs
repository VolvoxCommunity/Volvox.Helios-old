using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.Models;
using Volvox.Helios.Web.ViewModels.Settings;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/Settings/{guildId}")]
    [IsUserGuildAdminFilter]
    public class SettingsController : Controller
    {
        private readonly IModuleSettingsService<StreamAnnouncerSettings> _streamAnnouncerSettingsService;
        private readonly IModuleSettingsService<StreamerRoleSettings> _streamerRoleSettingsService;
        private readonly IEntityService<StreamAnnouncerChannelSettings> _streamAnnouncerChannelSettingsService;

        public SettingsController(IModuleSettingsService<StreamAnnouncerSettings> streamAnnouncerSettingsService,
            IModuleSettingsService<StreamerRoleSettings> streamerRoleSettingsService, IEntityService<StreamAnnouncerChannelSettings> streamAnnouncerChannelSettingsService)
        {    
            _streamAnnouncerSettingsService = streamAnnouncerSettingsService;
            _streamerRoleSettingsService = streamerRoleSettingsService;
            _streamAnnouncerChannelSettingsService = streamAnnouncerChannelSettingsService;
        }

        public IActionResult Index(ulong guildId, [FromServices] IBot bot,
            [FromServices] IDiscordSettings discordSettings, [FromServices] IList<IModule> modules)
        {
            if (bot.IsBotInGuild(guildId))
            {
                var viewModel = new SettingsIndexViewModel
                {
                    GuildId = guildId,
                    GuildName = bot.GetGuilds().FirstOrDefault(g => g.Id == guildId)?.Name,
                    Modules = modules.Where(mod => mod.Configurable).ToList()
                };

                return View(viewModel);
            }

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
            // All channels for guild
            var channels = await guildService.GetChannels(guildId);

            // Text channels for guild
            var textChannels = channels.Where(x => x.Type == 0).ToList();

            var viewModel = new StreamAnnouncerSettingsViewModel
            {
                Channels = new SelectList(textChannels, "Id", "Name"),
                ChannelSettings = new StreamAnnouncerChannelSettingsViewModel(),
                GuildId = guildId.ToString()
            };

            // Get general module settings for guild, from database
            var settings = await _streamAnnouncerSettingsService.GetSettingsByGuild(guildId, x => x.ChannelSettings);

            if (settings == null)
                return View(viewModel);

            viewModel.Enabled = settings.Enabled;

            // Gets first text channel's settings to prepopulate view with
            var defaultChannel = settings.ChannelSettings.FirstOrDefault(x => x.ChannelId == textChannels[0].Id);

            // No channels setting saved, return viewmodel as is.
            if (defaultChannel == null)
                return View(viewModel);

            viewModel.ChannelSettings.RemoveMessages = defaultChannel.RemoveMessage;

            // Channel settings only exist if the module is enabled.
            viewModel.ChannelSettings.Enabled = true;

            return View(viewModel);
        }

        // POST
        [HttpPost("StreamAnnouncer")]
        public async Task<IActionResult> StreamAnnouncerSettings(ulong guildId, StreamAnnouncerSettingsViewModel viewModel)
        {         
            var settings = await _streamAnnouncerChannelSettingsService.Find(viewModel.ChannelId);

            var isSettingsInDb = settings != null;

            var saveSettingsTasks = new List<Task>();

            // Save general module settings to the database
            saveSettingsTasks.Add(_streamAnnouncerSettingsService.SaveSettings(new StreamAnnouncerSettings
            {
                GuildId = guildId,
                Enabled = viewModel.Enabled,
            }));

            if (!isSettingsInDb)
            {
                settings = new StreamAnnouncerChannelSettings()
                {
                    GuildId = guildId,
                    ChannelId = viewModel.ChannelId
                };
            }

            settings.RemoveMessage = viewModel.ChannelSettings.RemoveMessages;

            // Save specific channel settings to the database
            if (viewModel.ChannelSettings.Enabled)
            {
                if (!isSettingsInDb)
                    saveSettingsTasks.Add(_streamAnnouncerChannelSettingsService.Create(settings));
                else
                    saveSettingsTasks.Add(_streamAnnouncerChannelSettingsService.Update(settings));
            }
            else
            {
                if(!isSettingsInDb)
                    saveSettingsTasks.Add(_streamAnnouncerChannelSettingsService.Remove(settings));
            }

            await Task.WhenAll(saveSettingsTasks.ToArray());

            return RedirectToAction("Index");
        }

        #endregion

        #region StreamerRole

        // GET
        [HttpGet("StreamerRole")]
        public async Task<IActionResult> StreamerRoleSettings(ulong guildId,
            [FromServices] IDiscordGuildService guildService)
        {
            var roles = await guildService.GetRoles(guildId);

            var viewModel = new StreamerRoleSettingsViewModel
            {
                Roles = new SelectList(roles.RemoveManaged(), "Id", "Name")
            };

            var settings = await _streamerRoleSettingsService.GetSettingsByGuild(guildId);

            if (settings == null)
                return View(viewModel);

            viewModel.RoleId = settings.RoleId;
            viewModel.Enabled = settings.Enabled;

            return View(viewModel);
        }

        // POST
        [HttpPost("StreamerRole")]
        public async Task<IActionResult> StreamerRoleSettings(ulong guildId, StreamerRoleSettingsViewModel viewModel, [FromServices] IBot bot, [FromServices] IDiscordGuildService guildService)
        {
            var botRolePosition = bot.GetBotRoleHierarchy(guildId);

            var roles = await guildService.GetRoles(guildId);

            var selectedRolePosition = roles.FirstOrDefault(r => r.Id == viewModel.RoleId)?.Position;

            // Discord bots cannot assign to roles that are higher then them in the hierarchy.
            if (selectedRolePosition > botRolePosition)
            {
                ModelState.AddModelError("RolePosition", "The bots managed role must be positioned higher then the selected role");

                viewModel.Roles = new SelectList(roles.RemoveManaged(), "Id", "Name");

                return View(viewModel);
            }

            // Save the settings to the database
            await _streamerRoleSettingsService.SaveSettings(new StreamerRoleSettings
            {
                GuildId = guildId,
                Enabled = viewModel.Enabled,
                RoleId = viewModel.RoleId
            });

            return RedirectToAction("Index");
        }

        #endregion
    }
}