using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.Extensions;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.ViewModels.Settings;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/Settings/{guildId}")]
    [IsUserGuildAdminFilter]
    public class SettingsController : Controller
    {
        private readonly IModuleSettingsService<ChatTrackerSettings> _chatTrackerSettingsService;
        private readonly IEntityService<RecurringReminderMessage> _recurringReminderService;
        private readonly IModuleSettingsService<RemembotSettings> _reminderSettingsService;
        private readonly IEntityService<StreamerChannelSettings> _streamAnnouncerChannelSettingsService;
        private readonly IModuleSettingsService<StreamerSettings> _streamAnnouncerSettingsService;

        public SettingsController(IModuleSettingsService<StreamerSettings> streamAnnouncerSettingsService,
            IEntityService<StreamerChannelSettings> streamAnnouncerChannelSettingsService,
            IModuleSettingsService<ChatTrackerSettings> chatTrackerSettingsService,
            IModuleSettingsService<RemembotSettings> reminderSettingsService,
            IEntityService<RecurringReminderMessage> recurringReminderService)
        {
            _streamAnnouncerSettingsService = streamAnnouncerSettingsService;
            _streamAnnouncerChannelSettingsService = streamAnnouncerChannelSettingsService;
            _chatTrackerSettingsService = chatTrackerSettingsService;
            _reminderSettingsService = reminderSettingsService;
            _recurringReminderService = recurringReminderService;
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

        #region Streamer

        // GET
        [HttpGet("Streamer")]
        public async Task<IActionResult> StreamerSettings(ulong guildId,
            [FromServices] IDiscordGuildService guildService)
        {
            // All channels in guild.
            var channels = await guildService.GetChannels(guildId);

            // Text channels in guild.
            var textChannels = channels.Where(x => x.Type == 0).ToList();

            // Roles in guild.
            var roles = await guildService.GetRoles(guildId);

            var viewModel = new StreamerSettingsViewModel
            {
                Channels = new SelectList(textChannels, "Id", "Name"),
                ChannelSettings = new StreamerChannelSettingsViewModel(),
                GuildId = guildId.ToString(),
                Roles = new SelectList(roles.RemoveManaged(), "Id", "Name")
            };

            // Get general module settings for guild, from database.
            var settings = await _streamAnnouncerSettingsService.GetSettingsByGuild(guildId, x => x.ChannelSettings);

            if (settings == null) return View(viewModel);

            viewModel.Enabled = settings.Enabled;
            viewModel.StreamerRoleEnabled = settings.StreamerRoleEnabled;
            viewModel.RoleId = settings.RoleId;

            // Gets first text channel's settings to prepopulate view with.
            var defaultChannel = settings.ChannelSettings?.FirstOrDefault(x => x.ChannelId == textChannels[0].Id);

            // No channels setting saved, return view model as is.
            if (defaultChannel == null) return View(viewModel);

            viewModel.ChannelSettings.RemoveMessages = defaultChannel.RemoveMessage;

            // Channel settings only exist if the module is enabled.
            viewModel.ChannelSettings.Enabled = true;

            viewModel.RoleId = settings.RoleId;
            viewModel.Enabled = settings.Enabled;

            return View(viewModel);
        }

        // POST
        [HttpPost("Streamer")]
        public async Task<IActionResult> StreamerSettings(ulong guildId,
            StreamerSettingsViewModel viewModel, [FromServices] IBot bot,
            [FromServices] IDiscordGuildService guildService)
        {
            var botRolePosition = bot.GetBotRoleHierarchy(guildId);

            var roles = await guildService.GetRoles(guildId);

            var selectedRolePosition = roles.FirstOrDefault(r => r.Id == viewModel.RoleId)?.Position;

            // Discord bots cannot assign to roles that are higher then them in the hierarchy.
            if (selectedRolePosition > botRolePosition)
            {
                ModelState.AddModelError("RolePosition",
                    "The bots managed role must be positioned higher then the selected role");

                // All channels in guild.
                var channels = await guildService.GetChannels(guildId);

                // Text channels in guild.
                var textChannels = channels.Where(x => x.Type == 0).ToList();

                viewModel.Channels = new SelectList(textChannels, "Id", "Name");
                viewModel.Roles = new SelectList(roles.RemoveManaged(), "Id", "Name");

                return View(viewModel);
            }

            var settings = await _streamAnnouncerChannelSettingsService.Find(viewModel.ChannelId);

            // Remember if there were settings in db, as settings will be populated later if they aren't.
            var isSettingsInDb = settings != null;

            var saveSettingsTasks = new List<Task>
            {
                _streamAnnouncerSettingsService.SaveSettings(new StreamerSettings
                {
                    GuildId = guildId,
                    Enabled = viewModel.Enabled,
                    StreamerRoleEnabled = viewModel.StreamerRoleEnabled,
                    RoleId = viewModel.RoleId
                })
            };

            // Save general module settings to the database
            if (!isSettingsInDb)
                settings = new StreamerChannelSettings
                {
                    GuildId = guildId,
                    ChannelId = viewModel.ChannelId
                };

            settings.RemoveMessage = viewModel.ChannelSettings.RemoveMessages;

            // Save specific channel settings to the database.
            if (viewModel.ChannelSettings.Enabled)
                saveSettingsTasks.Add(!isSettingsInDb
                    ? _streamAnnouncerChannelSettingsService.Create(settings)
                    : _streamAnnouncerChannelSettingsService.Update(settings));
            else
            {
                if (isSettingsInDb)
                    saveSettingsTasks.Add(_streamAnnouncerChannelSettingsService.Remove(settings));
            }

            await Task.WhenAll(saveSettingsTasks.ToArray());

            return RedirectToAction("Index");
        }

        #endregion

        #region ChatTracker

        // GET
        [HttpGet("ChatTracker")]
        public async Task<IActionResult> ChatTrackerSettings(ulong guildId)
        {
            var viewModel = new ChatTrackerSettingsViewModel();

            var settings = await _chatTrackerSettingsService.GetSettingsByGuild(guildId);

            if (settings == null)
                return View(viewModel);

            viewModel.Enabled = settings.Enabled;

            return View(viewModel);
        }

        // POST
        [HttpPost("ChatTracker")]
        public async Task<IActionResult> ChatTrackerSettings(ulong guildId, ChatTrackerSettingsViewModel viewModel)
        {
            // Save settings to the database.
            await _chatTrackerSettingsService.SaveSettings(new ChatTrackerSettings
            {
                GuildId = guildId,
                Enabled = viewModel.Enabled
            });

            return RedirectToAction("Index");
        }

        #endregion

        #region Reminder

        [HttpGet("Remembot")]
        public async Task<IActionResult> RemembotSettings(ulong guildId,
            [FromServices] IDiscordGuildService guildService)
        {
            var settings = await _reminderSettingsService.GetSettingsByGuild(guildId);
            var channels = await guildService.GetChannels(guildId);
            var textChannels = channels.Where(x => x.Type == 0).ToList();
            if (settings is null)
            {
                settings = new RemembotSettings
                {
                    Enabled = false,
                    GuildId = guildId
                };

                await _reminderSettingsService.SaveSettings(settings);
            }

            var reminders = await _recurringReminderService.Get(x => x.GuildId == guildId);
            settings.RecurringReminders = reminders;
            var viewModel = ReminderSettingsViewModel.FromData(settings, channels);
            return View(viewModel);
        }

        [HttpPost("Remembot")]
        public async Task<IActionResult> RemembotSettings(ulong guildId, ReminderSettingsViewModel reminderSettings)
        {
            await _reminderSettingsService.SaveSettings(new RemembotSettings
            {
                Enabled = reminderSettings.Enabled,
                GuildId = guildId
            });

            return RedirectToAction("Index");
        }

        #endregion
    }
}