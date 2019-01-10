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
        // TODO : MAKE SURE EXPIRE PERIOD DOESNT EXCEDE MAX DATETIME

        // TODO : add extra filters like caps filters, emoji filters

        // TODO : Why is the module not working properly sometimes? usually after I just boot up shit. perhaps because it hasnt finished loading or something? try and figure out what the problem is.

        // TODO : need to resave to enable filter? could be that its not saved, but the default value when loading a filter page is to be enabled so it looks enabled?

        /* Punishment service abstraction to do list:
         * 1) make sure addrolepunishment functions properly. see if way to make addpunishment not require socket guild user
         * 2) do similar class/service for each punishment (i.e. kick/ban...)
        */

        // TODO : subscribe to settings changed events etc in module. look at how remembot does it.

        // TODO : remove activepunishment first, if it fails, dont remove punishment and return a value to get hangfire to reschedule job or something?
        // ^ make sure to include performcontext (or whatever its called) into the method called by hangfire. RemovePunishment method in punishment service,

        // TODO : re-add functionality to Post information about punishment that is applied.

        // TODO : If removal of a punishment fails, dont readd the schdeduled job, but do keep active punishment. the admin will have to manually remove the punishments.

        // TODO : In addrolepunishment service, if guild is null, think about changing from not doing anyuthing to removing data about the guild as it was removed from the bot?

        // TODO : Consider extracting all sub services from startup into one main service, which can call all other services.

        // TODO : Change views to modern razer pages instead of @html stuff

        // TODO : Subscribe to onchannelcreated to configure muted role?

        // TODO NEXT DO THIS NEXT U BIC: Stop passing in moderation settings into services. have each service get the info themselves.


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

            await AnalyseWithFilters(settings, message);
        }

        private async Task AnalyseWithFilters(ModerationSettings settings, SocketMessage message)
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