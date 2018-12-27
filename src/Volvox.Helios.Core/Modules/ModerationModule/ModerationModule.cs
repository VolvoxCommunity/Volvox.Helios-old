using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Services.MessageService;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using System;
using Volvox.Helios.Service.EntityService;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Service.BackgroundJobs;
using Hangfire;
using Volvox.Helios.Core.Jobs;
using Volvox.Helios.Core.Modules.ModerationModule.Filters.Link;
using Volvox.Helios.Core.Modules.ModerationModule.Filters.Profanity;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService;
using Volvox.Helios.Core.Modules.ModerationModule.WarningService;
using Volvox.Helios.Core.Modules.ModerationModule.UserWarningsService;
using Volvox.Helios.Core.Modules.ModerationModule.Filters;
using Volvox.Helios.Core.Modules.ModerationModule.BypassCheck;

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        // TODO : extract banning and stuff into services

        // TODO : MAKE SURE EXPIRE PERIOD DOESNT EXCEDE MAX DATETIME

        // TODO : Consider not adding warnings if the guild has no punishments in place. Perhaps have purge button too, for warnings of users.

        // TODO : Add warnings page to clear user warnings and stuff

        // TODO : add extra filters like caps filters, emoji filters

        // TODO : Why is the module not working properly sometimes? usually after I just boot up shit. perhaps because it hasnt finished loading or something? try and figure out what the problem is.

        // TODO : Extract adding of punishments into their own service

        // TODO : Ensure bot has perms to remove a message before it does.

        // TODO : Why does redirect on users page fail after posting

        // TODO : Fix removal of punishments from posting on users page.

        // TODO : Extract logic and user interfaces, that way can change the functionality without changing the core module

        // TODO : Add prev/next buttons to users page

        // TODO : Finish punishment service stuff like remove punishments

        // TODO : Possible caching issue. Sometimes moduel things profanity/link filter is null. needing a restart to fix. wtf?

        // TODO : Further abstract logic in punishment service. Too much in one class atm, think of making the actual banning/kicking its own separate class.

        #region Private vars

        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        private readonly IMessageService _messageService;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IJobService _jobService;

        private readonly IFilterService<LinkFilter> _linkFilterService;

        private readonly IFilterService<ProfanityFilter> _profanityFilterService;

        private readonly IBypassCheck _bypassCheck;

        private readonly IPunishmentService _punishmentService;

        private readonly IWarningService _warningService;

        private readonly IUserWarningsService _userWarningService;

        #endregion

        public ModerationModule(IDiscordSettings discordSettings, ILogger<ModerationModule> logger,
            IConfiguration config, IModuleSettingsService<ModerationSettings> settingsService,
            IMessageService messageService, IServiceScopeFactory scopeFactory, IJobService jobservice,
            IFilterService<LinkFilter> linkFilterService, IFilterService<ProfanityFilter> profanityFilterService,
            IPunishmentService punishmentService, IWarningService warningService, IBypassCheck bypassCheck,
            IUserWarningsService userWarningService
        ) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;

            _messageService = messageService;

            _scopeFactory = scopeFactory;

            _jobService = jobservice;

            _linkFilterService = linkFilterService;

            _profanityFilterService = profanityFilterService;

            _bypassCheck = bypassCheck;

            _punishmentService = punishmentService;

            _warningService = warningService;

            _userWarningService = userWarningService;

            
        }

        public override Task Init(DiscordSocketClient client)
        {
            client.MessageReceived += async message =>
            {
                await CheckMessage(message);
            };

            client.MessageUpdated += async (cache, message, channel) =>
            {
                await CheckMessage(message);
            };

            using (var scope = _scopeFactory.CreateScope())
            {
                var warningService = scope.ServiceProvider.GetRequiredService<RemoveExpiredWarningsJob>();

                _jobService.ScheduleRecurringJob(() => warningService.Run(), Cron.Minutely(), "RemoveExpiredWarnings");
            }

            return Task.CompletedTask;
        }

        private async Task CheckMessage(SocketMessage message)
        {
            // Message can be null sometimes when dealing with deleted messages
            if (message is null)
                return;

            var user = message.Author as SocketGuildUser;

            // Get all relevant data from database using navigation properties.
            var settings = await _settingsService.GetSettingsByGuild(user.Guild.Id,
                s => s.ProfanityFilter.BannedWords, s => s.LinkFilter.WhitelistedLinks, s => s.Punishments, s => s.WhitelistedChannels, s => s.WhitelistedRoles
            );

            // Settings will be null if users haven't done anything with the moderation module.
            // If settings are null, or settings isn't enabled, then the module isn't enabled. Do nothing.
            if (settings is null || !settings.Enabled)

            await AnalyseWithFilters(settings, message);
        }

        private async Task AnalyseWithFilters(ModerationSettings settings, SocketMessage message)
        {
            if (_bypassCheck.HasBypassAuthority(settings, message, WhitelistType.Global))

            // Return if true as we don't want to bother checking for links if the message already violates the profanity filter.
            if (await ProfanityFilterHandler(settings, message))
                return;

            if (await LinkFilterHandler(settings, message))
                return;   
        }

        private async Task<bool> ProfanityFilterHandler(ModerationSettings settings, SocketMessage message)
        {
            var author = message.Author as SocketGuildUser;

            if (ProfanityCheck(settings, message))
            {
                await HandleViolation(settings, message, author, WarningType.Profanity);

                return true;
            }

            return false;
        }

        private async Task<bool> LinkFilterHandler(ModerationSettings settings, SocketMessage message)
        {
            var author = message.Author as SocketGuildUser;

            if (LinkCheck(settings, message))
            {
                await HandleViolation(settings, message, author, WarningType.Link);

                return true;
            }

            return false;
        }

        private bool ProfanityCheck(ModerationSettings settings, SocketMessage message)
        {
            var filterViolatedFlag = false;

            if (!_bypassCheck.HasBypassAuthority(settings, message, WhitelistType.Profanity))
            {
                if (_profanityFilterService.CheckViolation(settings.ProfanityFilter, message))
                    filterViolatedFlag = true;
            }

            return filterViolatedFlag;
        }

        private bool LinkCheck(ModerationSettings settings, SocketMessage message)
        {
            var filterViolatedFlag = false;

            if (!_bypassCheck.HasBypassAuthority(settings, message, WhitelistType.Link))
            {
                if (_linkFilterService.CheckViolation(settings.LinkFilter, message))
                    filterViolatedFlag = true;
            }

            return filterViolatedFlag;
        }

        private async Task HandleViolation(ModerationSettings moderationSettings, SocketMessage message, SocketGuildUser user, WarningType warningType)
        {
            await message.DeleteAsync();

            await _messageService.Post(message.Channel.Id, $"Message by <@{user.Id}> deleted\nReason: {warningType}");

            var userData = await _userWarningService.GetUser(user.Id, user.Guild.Id, u => u.Warnings, u => u.ActivePunishments);

            // Add warning to database.
            var newWarning = await _warningService.AddWarning(moderationSettings, user, warningType);

            // Update cached version.
            userData.Warnings.Add(newWarning);

            // Get all warnings that haven't expired.
            var userWarnings = userData.Warnings.Where(x => x.WarningExpires > DateTimeOffset.Now);

            // Count warnings of violation type.
            var specificWarningCount = userWarnings.Count(x => x.WarningType == warningType);

            // Count total number of warnings.
            var allWarningsCount = userWarnings.Count();

            var punishments = new List<Punishment>();

            // Global punishments
            punishments.AddRange(moderationSettings.Punishments.Where(x => x.WarningType == WarningType.General && x.WarningThreshold == allWarningsCount));

            // Punishments for specific type. I.E. profanity violation.
            punishments.AddRange(moderationSettings.Punishments.Where(x => x.WarningType == warningType && x.WarningThreshold == specificWarningCount));

            await _punishmentService.ApplyPunishments(moderationSettings, message.Channel.Id, punishments, user);
        }
 
        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var settings = await _settingsService.GetSettingsByGuild(guildId);

            return settings != null && settings.Enabled;
        }
    }
}