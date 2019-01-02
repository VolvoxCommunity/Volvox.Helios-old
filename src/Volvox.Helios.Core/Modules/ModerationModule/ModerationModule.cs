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
        // TODO : MAKE SURE EXPIRE PERIOD DOESNT EXCEDE MAX DATETIME

        // TODO : add extra filters like caps filters, emoji filters

        // TODO : Why is the module not working properly sometimes? usually after I just boot up shit. perhaps because it hasnt finished loading or something? try and figure out what the problem is.

        // TODO : Why does redirect on users page fail after posting

        // TODO : Add prev/next buttons to users page

        // TODO : Further abstract logic in punishment service. Too much in one class atm, think of making the actual banning/kicking its own separate class.

        // TODO : consider making GlobalFilter instead of checking for global in this class.

        // TODO : Try to refactor so dont have to pass moderation settings into filter classes, just the filter. maybe add reference to warnings/punishments to the filter?

        // TODO : need to resave to enable filter? could be that its not saved, but the default value when loading a filter page is to be enabled so it looks enabled?

        #region Private vars

        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IJobService _jobService;

        private readonly IFilterService<LinkFilter> _linkFilterService;

        private readonly IFilterService<ProfanityFilter> _profanityFilterService;

        private readonly IBypassCheck _bypassCheck;

        #endregion

        public ModerationModule(IDiscordSettings discordSettings, ILogger<ModerationModule> logger,
            IConfiguration config, IModuleSettingsService<ModerationSettings> settingsService, IServiceScopeFactory scopeFactory,
            IJobService jobService, IFilterService<LinkFilter> linkFilterService,
            IFilterService<ProfanityFilter> profanityFilterService,IBypassCheck bypassCheck
        ) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;

            _linkFilterService = linkFilterService;

            _profanityFilterService = profanityFilterService;

            _scopeFactory = scopeFactory;

            _jobService = jobService;

            _bypassCheck = bypassCheck;
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
                return;

            await AnalyseWithFilters(settings, message);
        }

        private async Task AnalyseWithFilters(ModerationSettings settings, SocketMessage message)
        {
            if (_bypassCheck.HasBypassAuthority(settings, message, WhitelistType.Global))
                return;

            // Return if true as we don't want to bother checking for links if the message already violates previous filters.
            if (_profanityFilterService.CheckViolation(settings, message))
            {
                await _profanityFilterService.HandleViolation(settings, message);
            }
            else if (_linkFilterService.CheckViolation(settings, message))
            {
                await _linkFilterService.HandleViolation(settings, message);
            }
        }
        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var settings = await _settingsService.GetSettingsByGuild(guildId);

            return settings != null && settings.Enabled;
        }
    }
}