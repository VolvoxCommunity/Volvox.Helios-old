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
        private readonly IEntityService<StreamAnnouncerChannelSettings> _streamAnnouncerChannelSettingsService;
        private readonly IModuleSettingsService<StreamAnnouncerSettings> _streamAnnouncerSettingsService;
        private readonly IModuleSettingsService<StreamerRoleSettings> _streamerRoleSettingsService;
        private readonly IModuleSettingsService<ReminderSettings> _reminderSettingsService;
        private readonly IEntityService<RecurringReminderMessage> _recurringReminderService;

        public SettingsController(IModuleSettingsService<StreamAnnouncerSettings> streamAnnouncerSettingsService,
            IModuleSettingsService<StreamerRoleSettings> streamerRoleSettingsService,
            IEntityService<StreamAnnouncerChannelSettings> streamAnnouncerChannelSettingsService,
            IModuleSettingsService<ChatTrackerSettings> chatTrackerSettingsService,
            IModuleSettingsService<ReminderSettings> reminderSettingsService,
            IEntityService<RecurringReminderMessage> recurringReminderService)
        {
            _streamAnnouncerSettingsService = streamAnnouncerSettingsService;
            _streamerRoleSettingsService = streamerRoleSettingsService;
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

        #region StreamAnnouncer

        // GET
        [HttpGet("StreamAnnouncer")]
        public async Task<IActionResult> StreamAnnouncerSettings(ulong guildId,
            [FromServices] IDiscordGuildService guildService)
        {
            // All channels for guild.
            var channels = await guildService.GetChannels(guildId);

            // Text channels for guild.
            var textChannels = channels.Where(x => x.Type == 0).ToList();

            var viewModel = new StreamAnnouncerSettingsViewModel
            {
                Channels = new SelectList(textChannels, "Id", "Name"),
                ChannelSettings = new StreamAnnouncerChannelSettingsViewModel(),
                GuildId = guildId.ToString()
            };

            // Get general module settings for guild, from database.
            var settings = await _streamAnnouncerSettingsService.GetSettingsByGuild(guildId, x => x.ChannelSettings);

            if (settings == null) return View(viewModel);

            viewModel.Enabled = settings.Enabled;

            // Gets first text channel's settings to prepopulate view with.
            var defaultChannel = settings.ChannelSettings.FirstOrDefault(x => x.ChannelId == textChannels[0].Id);

            // No channels setting saved, return viewmodel as is.
            if (defaultChannel == null) return View(viewModel);

            viewModel.ChannelSettings.RemoveMessages = defaultChannel.RemoveMessage;

            // Channel settings only exist if the module is enabled.
            viewModel.ChannelSettings.Enabled = true;

            return View(viewModel);
        }

        // POST
        [HttpPost("StreamAnnouncer")]
        public async Task<IActionResult> StreamAnnouncerSettings(ulong guildId,
            StreamAnnouncerSettingsViewModel viewModel)
        {
            var settings = await _streamAnnouncerChannelSettingsService.Find(viewModel.ChannelId);

            // Remember if there were settings in db, as settings will be populated later if they aren't.
            var isSettingsInDb = settings != null;

            var saveSettingsTasks = new List<Task>
            {
                _streamAnnouncerSettingsService.SaveSettings(new StreamAnnouncerSettings
                {
                    GuildId = guildId,
                    Enabled = viewModel.Enabled
                })
            };

            // Save general module settings to the database

            if (!isSettingsInDb)
                settings = new StreamAnnouncerChannelSettings
                {
                    GuildId = guildId,
                    ChannelId = viewModel.ChannelId
                };

            settings.RemoveMessage = viewModel.ChannelSettings.RemoveMessages;

            // Save specific channel settings to the database.
            if (viewModel.ChannelSettings.Enabled)
            {
                saveSettingsTasks.Add(!isSettingsInDb
                    ? _streamAnnouncerChannelSettingsService.Create(settings)
                    : _streamAnnouncerChannelSettingsService.Update(settings));
            }
            else
            {
                if (isSettingsInDb)
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

            if (settings == null) return View(viewModel);

            viewModel.RoleId = settings.RoleId;
            viewModel.Enabled = settings.Enabled;

            return View(viewModel);
        }

        // POST
        [HttpPost("StreamerRole")]
        public async Task<IActionResult> StreamerRoleSettings(ulong guildId, StreamerRoleSettingsViewModel viewModel,
            [FromServices] IBot bot, [FromServices] IDiscordGuildService guildService)
        {
            var botRolePosition = bot.GetBotRoleHierarchy(guildId);

            var roles = await guildService.GetRoles(guildId);

            var selectedRolePosition = roles.FirstOrDefault(r => r.Id == viewModel.RoleId)?.Position;

            // Discord bots cannot assign to roles that are higher then them in the hierarchy.
            if (selectedRolePosition > botRolePosition)
            {
                ModelState.AddModelError("RolePosition",
                    "The bots managed role must be positioned higher then the selected role");

                viewModel.Roles = new SelectList(roles.RemoveManaged(), "Id", "Name");

                return View(viewModel);
            }

            // Save the settings to the database.
            await _streamerRoleSettingsService.SaveSettings(new StreamerRoleSettings
            {
                GuildId = guildId,
                Enabled = viewModel.Enabled,
                RoleId = viewModel.RoleId
            });

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
        public async Task<IActionResult> ChatTrackerSettings(ulong guildId, StreamerRoleSettingsViewModel viewModel)
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
        [HttpGet("Reminder")]
        public async Task<IActionResult> ReminderSettings(ulong guildId)
        {
            var settings = await _reminderSettingsService.GetSettingsByGuild(guildId, rs => rs.RecurringReminders);
            var viewModel = ReminderSettingsViewModel.FromData(settings);
            return View(viewModel);
        }

        [HttpPost("Reminder")]
        public async Task<IActionResult> ReminderSettings(ulong guildId, ReminderSettingsViewModel reminderSettings)
        {
            await _reminderSettingsService.SaveSettings(new ReminderSettings
            {
                Enabled = reminderSettings.Enabled,
                GuildId = guildId
            });

            return RedirectToAction("Index");
        }

        [HttpGet("EditRecurringReminder")]
        public async Task<IActionResult> EditRecurringReminder(ulong guildId, [FromQuery]ulong rid)
        {
            var reminder = await _recurringReminderService.Find(rid);

            if (reminder is null)
                return View(new RecurringReminderMessageViewModel());

            var vm = new RecurringReminderMessageViewModel
            {
                ChannelId = reminder.ChannelId,
                Enabled = reminder.Enabled,
                Id = reminder.Id,
                Message = reminder.Message,
                Recurrence = reminder.ReadableCronExpression
            };

            return View(vm);
        }

        [HttpPost("EditRecurringReminder")]
        public async Task<IActionResult> EditRecurringReminder(ulong guildId, RecurringReminderMessageViewModel vm)
        {

            return RedirectToAction("ReminderSettings");
        }
        #endregion
    }
}