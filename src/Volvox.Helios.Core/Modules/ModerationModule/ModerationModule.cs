using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Jobs;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Modules.ModerationModule.BypassCheck;
using Volvox.Helios.Core.Modules.ModerationModule.Filters;
using Volvox.Helios.Core.Modules.ModerationModule.Utils;
using Volvox.Helios.Core.Modules.ModerationModule.ViolationService;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Service.BackgroundJobs;

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        #region Private vars
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IJobService _jobService;

        private readonly IEnumerable<IFilterService> _filters;

        private readonly IBypassCheck _bypassCheck;

        private readonly IModerationModuleUtils _moderationModuleUtils;

        private readonly IViolationService _violationService;

        #endregion

        public ModerationModule(IDiscordSettings discordSettings, ILogger<ModerationModule> logger,
            IConfiguration config, IServiceScopeFactory scopeFactory,
            IJobService jobService, IEnumerable<IFilterService> filters, IBypassCheck bypassCheck, IModerationModuleUtils moderationModuleUtils,
            IViolationService violationService
        ) : base(
            discordSettings, logger, config)
        {
            _scopeFactory = scopeFactory;

            _jobService = jobService;

            _filters = filters;

            _bypassCheck = bypassCheck;

            _moderationModuleUtils = moderationModuleUtils;

            _violationService = violationService;
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
                    await _violationService.HandleViolation(message, filter.GetFilterMetaData().FilterType);

                    // Don't check for more violations if already found one in previous filter.
                    break;
                }
            }
        }
        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings(guildId);

            return settings != null && settings.Enabled;
        }   
    }
}