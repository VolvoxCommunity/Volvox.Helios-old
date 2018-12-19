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

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        // TODO : extract banning and stuff into services

        // TODO : MAKE SURE EXPIRE PERIOD DOESNT EXCEDE MAX DATETIME

        // TODO : Consider not adding warnings if the guild has no punishments in place. Perhaps have purge button too, for warnings of users.

        // TODO : Add warnings page to clear user warnings and stuff

        // TODO : add extra filters like caps filters, emoji filters

        // TODO : Extract logic and user interfaces, that way can change the functionality without changing the core module

        #region Private vars

        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        private readonly IMessageService _messageService;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IJobService _jobService;

        private readonly ILinkFilterService _linkFilterService;

        private readonly IProfanityFilterService _profanityFilterService;

        private readonly IPunishmentService _punishmentService;

        private readonly IWarningService _warningService;

        #endregion

        public ModerationModule(IDiscordSettings discordSettings, ILogger<ModerationModule> logger,
            IConfiguration config, IModuleSettingsService<ModerationSettings> settingsService,
            IMessageService messageService, IServiceScopeFactory scopeFactory, IJobService jobservice,
            ILinkFilterService linkFilterService, IProfanityFilterService profanityFilterService,
            IPunishmentService punishmentService, IWarningService warningService
        ) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;
             
            _messageService = messageService;

            _scopeFactory = scopeFactory;

            _jobService = jobservice;

            _linkFilterService = linkFilterService;

            _profanityFilterService = profanityFilterService;

            _punishmentService = punishmentService;

            _warningService = warningService;
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

            var channelPostedId = message.Channel.Id;

            // If the user or channel is globally whitelisted, there is no point in checking the message contents.
            if (HasBypassAuthority(user, channelPostedId, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Global),
                settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Global)))
                return;

            // Do nothing if the filter doesn't exist for the guild or it's disabled.
            if (( settings?.ProfanityFilter != null ) && ( settings?.ProfanityFilter?.Enabled ?? false ))
            {
                var whitelistedChannels = settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Profanity) ?? new List<WhitelistedChannel>();

                var whitelistedRoles = settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Profanity);

                if (!HasBypassAuthority(user, channelPostedId, whitelistedChannels, whitelistedRoles)
                    && _profanityFilterService.ProfanityCheck(message, settings.ProfanityFilter))
                {
                    await HandleViolation(settings, message, user, WarningType.Profanity);
                    return;
                }
            }

            // Do nothing if the filter doesn't exist for a the guild or it's disabled.
            if ((settings.LinkFilter != null) && (settings.LinkFilter.Enabled))
            {
                var whitelistedChannels = settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Link);

                var whitelistedRoles = settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Link);

                if (!HasBypassAuthority(user, channelPostedId, whitelistedChannels, whitelistedRoles) &&
                   _linkFilterService.LinkCheck(message, settings.LinkFilter))
                {
                    await HandleViolation(settings, message, user, WarningType.Link);
                    return;
                }
            }      
        }

        private bool HasBypassAuthority(SocketGuildUser user, ulong postedChannelId,
            IEnumerable<WhitelistedChannel> whitelistedChannels, IEnumerable<WhitelistedRole> whitelistedRoles)
        {
            // Bots bypass check.
            if (user.IsBot) return true;

            // Check if channel id is whitelisted.
            if (whitelistedChannels.Any(x => x.ChannelId == postedChannelId)) return true;

            // Check for whitelisted role.
            if (user.Roles.Any(r => whitelistedRoles.Any(w => w.RoleId == r.Id))) return true;

            return false;
        }

        private async Task HandleViolation(ModerationSettings moderationSettings, SocketMessage message, SocketGuildUser user, WarningType warningType)
        {
            await message.DeleteAsync();

            await _messageService.Post(message.Channel.Id, $"Message by <@{user.Id}> deleted\nReason: {warningType}");

            UserWarnings userData;

            using (var scope = _scopeFactory.CreateScope())
            {
                var userWarningService = scope.ServiceProvider.GetRequiredService<IEntityService<UserWarnings>>();

                var userDataDb = await userWarningService.GetFirst(u => u.UserId == user.Id, u => u.Warnings, u => u.ActivePunishments);

                // User isn't tracked yet, so create new entry for them.
                if (userDataDb == null )
                {
                    userData = new UserWarnings()
                    {
                        GuildId = moderationSettings.GuildId,
                        UserId = user.Id
                    };

                    await userWarningService.Create(userData);
                }
                else
                {
                    userData = userDataDb;
                }
            }

            // Add warning to database.
            await _warningService.AddWarning(moderationSettings, user, userData, warningType);

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

            await _punishmentService.ApplyPunishments(moderationSettings, message.Channel.Id, punishments, user, userData);
        }
 
        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var settings = await _settingsService.GetSettingsByGuild(guildId);

            return settings != null && settings.Enabled;
        }
    }
}