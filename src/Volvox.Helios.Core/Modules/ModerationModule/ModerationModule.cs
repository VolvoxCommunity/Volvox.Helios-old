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
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Volvox.Helios.Core.Modules.ModerationModule.Utils;

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        // TODO : re-add functionality to Post information about punishment that is applied.

        // TODO : If removal of a punishment fails, dont readd the schdeduled job, but do keep active punishment. the admin will have to manually remove the punishments.

        // TODO : Change views to modern razer pages instead of @html stuff

        // TODO : Subscribe to onchannelcreated to configure muted role?

        // TODO : Remove such a specific implementation of SocketMessage. make own class and populate it using socketmessage.
        // ^ TODO : Requires guild id and user id, from there i can refetch details inside services. This means if we change library, functionality shouldn't change.

        #region Private vars

        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IJobService _jobService;

        private readonly IList<IFilterService> _filters;

        private readonly IBypassCheck _bypassCheck;

        private readonly IModerationModuleUtils _moderationModuleUtils;

        #endregion

        public ModerationModule(IDiscordSettings discordSettings, ILogger<ModerationModule> logger,
            IConfiguration config, IModuleSettingsService<ModerationSettings> settingsService, IServiceScopeFactory scopeFactory,
            IJobService jobService, IList<IFilterService> filters, IBypassCheck bypassCheck, IModerationModuleUtils moderationModuleUtils
        ) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;

            _scopeFactory = scopeFactory;

            _jobService = jobService;

            _filters = filters;

            _bypassCheck = bypassCheck;

            _moderationModuleUtils = moderationModuleUtils;
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
            var settings = await _moderationModuleUtils.GetModerationSettings(user.Guild.Id);

            // Settings will be null if users haven't done anything with the moderation module.
            // If settings are null, or settings isn't enabled, then the module isn't enabled. Do nothing.
            if (settings is null || !settings.Enabled)
                return;

            await AnalyseWithFilters(message);
        }

        private async Task AnalyseWithFilters(SocketMessage message)
        {
            if (await _bypassCheck.HasBypassAuthority(message, FilterType.Global))
                return;

            foreach(var filter in _filters)
            {
                if (await filter.CheckViolation(message))
                {
                    await filter.HandleViolation(message);

                    // Don't check for more violations if already found one in previous filter.
                    break;
                }
            }
        }
        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var settings = await _settingsService.GetSettingsByGuild(guildId);

            return settings != null && settings.Enabled;
        }   
    }
}