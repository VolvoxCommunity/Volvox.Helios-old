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

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        // TODO : extract banning and stuff into services

        // TODO : MAKE SURE EXPIRE PERIOD DOESNT EXCEDE MAX DATETIME

        // TODO : Uncomment out the auth checks in moderation controller.

        // TODO : Consider not adding warnings if the guild has no punishments in place. Perhaps have purge button too, for warnings of users.

        // TODO : display role name instead of id in punishments view

        // TODO : Add warnings page to clear user warnings and stuff

        // TODO : add extra filters like caps filters, emoji filters

        ///////////////////////// IMPORTANT TODO :  Fix null errors in the call to has bypass authority. if settings.whitelistedchannels/roles is null, there is an error.

        // TODO : Extract logic and user interfaces, that way can change the functionality without changing the core module


        #region Private vars

        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        private readonly IMessageService _messageService;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IJobService _jobService;

        private DiscordSocketClient _client;

        private readonly List<string> _defaultBannedWords = new List<string>();

        #endregion

        public ModerationModule(IDiscordSettings discordSettings, ILogger<ModerationModule> logger,
            IConfiguration config, IModuleSettingsService<ModerationSettings> settingsService,
            IMessageService messageService, IServiceScopeFactory scopeFactory, IJobService jobservice
        ) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;
             
            _messageService = messageService;

            _scopeFactory = scopeFactory;

            _jobService = jobservice;

            var defaultBannedWords = config.GetSection("BannedWords").GetChildren().Select(x => x.Value);

            if (defaultBannedWords != null)
            {
                _defaultBannedWords.AddRange(defaultBannedWords);
            }
        }

        public override Task Init(DiscordSocketClient client)
        {
            // As client is passed into init, no point in assigning client in constructor.
            _client = client;

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

                if (!HasBypassAuthority(user, channelPostedId, whitelistedChannels, whitelistedRoles) && ProfanityCheck(message, settings.ProfanityFilter))
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

                if (!HasBypassAuthority(user, channelPostedId, whitelistedChannels, whitelistedRoles) && LinkCheck(message, settings.LinkFilter))
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

        private bool ProfanityCheck(SocketMessage message, ProfanityFilter profanityFilter)
        {
            // Normalize message to lowercase and split into array of words.
            var messageWords = message.Content.ToLower().Split(" ");

            var bannedWords = profanityFilter.BannedWords.Select(w => w.Word).ToList();

            // Check for default banned words if UserDefaultList enabled.
            if (profanityFilter.UseDefaultList)
                bannedWords.AddRange(_defaultBannedWords);

            foreach (var word in messageWords)
            {
                foreach (var bannedWord in bannedWords)
                {
                    if (word == bannedWord) return true;
                }
            }

            return false;
        }

        private string ConvertClbuttic(string word)
        {
            return word.Replace("[a]", "[a A @]")
                .Replace("[b]", "[b B I3 l3 i3]")
                .Replace("[c]", "(?:[c C \\(]|[k K])")
                .Replace("[d]", "[d D]")
                .Replace("[e]", "[e E 3]")
                .Replace("[f]", "(?:[f F]|[ph pH Ph PH])")
                .Replace("[g]", "[g G 6]")
                .Replace("[h]", "[h H]")
                .Replace("[i]", "[i I l ! 1]")
                .Replace("[j]", "[j J]")
                .Replace("[k]", "(?:[c C \\(]|[k K])")
                .Replace("[l]", "[l L 1 ! i]")
                .Replace("[m]", "[m M]")
                .Replace("[n]", "[n N]")
                .Replace("[o]", "[o O 0]")
                .Replace("[p]", "[p P]")
                .Replace("[q]", "[q Q 9]")
                .Replace("[r]", "[r R]")
                .Replace("[s]", "[s S $ 5]")
                .Replace("[t]", "[t T 7]")
                .Replace("[u]", "[u U v V]")
                .Replace("[v]", "[v V u U]")
                .Replace("[w]", "[w W vv VV]")
                .Replace("[x]", "[x X]")
                .Replace("[y]", "[y Y]");
        }

        private bool LinkCheck(SocketMessage message, LinkFilter linkFilter)
        {
            // Normalize message to lowercase and split into array of words.
            var messageWords = message.Content.ToLower().Split(" ");

            var whitelistedLinks = linkFilter.WhitelistedLinks.Select(l => l.Link);

            // Regular expression for detecting url patterns
            var urlCheck = new Regex(@"[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)");

            // Flag for tracking whether current url is whitelisted
            var isLinkLegal = false;

            // Check each word for illegal link
            foreach (var word in messageWords)
            {
                // TODO: In future version of this module, check if url entered is of the same base, and not just matches exactly.
                if (urlCheck.IsMatch(word))
                {
                    foreach (var link in whitelistedLinks)
                    {
                        if (link == word)
                        {
                            isLinkLegal = true;
                            break;
                        }
                    }

                    if (!isLinkLegal) return true;
                                                 
                    isLinkLegal = false;
                }
            }       
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

                var listUserData = await userWarningService.Get(u => u.UserId == user.Id, u => u.Warnings, u => u.ActivePunishments);

                // User isn't tracked yet, so create new entry for them.
                if (listUserData.Count == 0)
                {
                    userData = new UserWarnings()
                    {
                        GuildId = moderationSettings.GuildId,
                        UserId = user.Id,
                        ActivePunishments = new List<ActivePunishment>(),
                        Warnings = new List<Warning>()
                    };

                    await userWarningService.Create(userData);
                }
                else
                {
                    userData = listUserData[0];
                }
            }

            // Add warning to database.
            await AddWarning(moderationSettings, user, userData, warningType);

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

            await ApplyPunishments(moderationSettings, message.Channel.Id, punishments, user, userData);
        }
 
        private async Task AddWarning(ModerationSettings moderationSettings, SocketGuildUser user, UserWarnings userData, WarningType warningType)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var warningService = scope.ServiceProvider.GetRequiredService<IEntityService<Warning>>();

                var specificWarningDuration = GetWarningDuration(moderationSettings, warningType);

                var expireDate = DateTimeOffset.Now.AddMinutes(specificWarningDuration);

                // 0 means punishment lasts forever.
                if (specificWarningDuration == 0)
                    expireDate = DateTimeOffset.MaxValue;

                var warning = new Warning()
                {
                    UserId = userData.Id,
                    WarningRecieved = DateTimeOffset.Now,
                    WarningExpires = expireDate,
                    WarningType = warningType
                };

                if (userData.Warnings == null) userData.Warnings = new List<Warning>();

                userData.Warnings.Add(warning);

                await warningService.Create(warning);

                Logger.LogInformation($"Moderation Module: User {user.Username} warned. Added warning to database. " +
                    $"Guild Id: {user.Guild.Id}, User Id: {user.Id}");
            }
        }

        private int GetWarningDuration(ModerationSettings moderationSettings, WarningType warningType)
        {
            var duration = 0;

            switch (warningType)
            {
                case ( WarningType.Link ):
                    duration = moderationSettings.LinkFilter.WarningExpirePeriod;
                    break;
                case ( WarningType.Profanity ):
                    duration = moderationSettings.ProfanityFilter.WarningExpirePeriod;
                    break;
            }

            return duration;
        }

        private async Task ApplyPunishments(ModerationSettings moderationSettings, ulong channelId, List<Punishment> punishments, SocketGuildUser user, UserWarnings userData)
        {
            var userHasBeenRemoved = false;

            // List of punishments to add to database as active punishments.
            var activePunishments = new List<Punishment>();

            foreach (var punishment in punishments)
            {
                // If a user has been kicked/banned or otherwise removed from the guild, you can't add any other punishments. So return from this method.
                if (userHasBeenRemoved)
                    return;

                // Check to make sure user doesn't already have this punishment. This could cause issues if the same punishment is applied twice.
                if (IsPunishmentAlreadyActive(punishment, userData))
                    continue;

                var wasSuccessful = true;

                // For each punishment, check if applying them has been successful. If successful, add this to active punishments db, if not, don't add it.
                // If a punishment removes the user from the guild, no more punisments can be applied to them, so don't attempt any more.
                switch (punishment.PunishType)
                {
                    case ( PunishType.Kick ):
                        if (await KickPunishment(punishment, channelId, user))            
                            userHasBeenRemoved = true;
                        else
                            wasSuccessful = false;
                        break;

                    case ( PunishType.Ban ):
                        if (await BanPunishment(punishment, channelId, user))
                            userHasBeenRemoved = true;
                        else
                            wasSuccessful = false;
                        break;

                    case ( PunishType.AddRole ):
                        if (await AddRolePunishment(punishment, channelId, user))
                        {}
                        else
                            wasSuccessful = false;
                        break;
                }

                // Punishment applied successfully, add to list to add to db.
                if (wasSuccessful)
                    activePunishments.Add(punishment);

                wasSuccessful = true;
            }

            await AddActivePunishments(moderationSettings, activePunishments, user, userData);
        }

        private async Task<bool> AddRolePunishment(Punishment punishment, ulong channelId, SocketGuildUser user)
        {
            if (!punishment.RoleId.HasValue)
                return false;

            var guild = user.Guild;

            var role = guild.GetRole(punishment.RoleId.Value);

            if (guild is null || role is null)
                return false;

            var hierarchy = _client.GetGuild(user.Guild.Id)?.CurrentUser.Hierarchy ?? 0;

            // Trying to assign a role higher than the bots hierarchy will throw an error.
            if (role.Position < hierarchy)
            {
                await user.AddRoleAsync(role);

                var expireTime = punishment.PunishDuration == null ? "Never" : punishment.PunishDuration.ToString();

                await _messageService.Post(channelId, $"Adding role '{role.Name}' to user <@{user.Id}>" +
                    $"\nReason: {punishment.WarningType}\n" +
                    $"Expires (minutes): {expireTime}");
            }
            else
            {
                Logger.LogInformation($"Moderation Module: Couldn't apply role '{role.Name}' to user '{user.Username}' as bot doesn't have appropriate permissions. " +
                    $"Guild Id:{user.Guild.Id}, Role Id: {punishment.RoleId.Value}, User Id: {user.Id}.");

                await _messageService.Post(channelId, $"Couldn't add role '{role.Name}' to user <@{user.Id}> as bot has insufficient permissions. " +
                    $"Check your role hierarchy and make sure the bot is higher than the role you wish to apply.");

                return false;
            }

            return true;
        }

        private async Task<bool> KickPunishment(Punishment punishment, ulong channelId, SocketGuildUser user)
        {                
            var botHierarchy = _client.GetGuild(user.Guild.Id)?.CurrentUser.Hierarchy ?? 0;

            if (botHierarchy > user.Hierarchy)
            {
                await user.KickAsync();

                Logger.LogInformation($"Moderation Module: Kicking user '{user.Username}'." +
                        $"Guild Id:{user.Guild.Id}, User Id: {user.Id}.");

                await _messageService.Post(channelId, $"Kicking user <@{user.Id}>" +
                    $"\nReason: {punishment.WarningType}");

                return true;
            }
            else
            {
                Logger.LogInformation($"Moderation Module: Failed to kick user '{user.Username}' as bot has insufficient permissions." +
                        $"Guild Id:{user.Guild.Id}, User Id: {user.Id}.");

                await _messageService.Post(channelId, $"Failed to kick user <@{user.Id}> as bot has insufficient permissions.");

                return false;
            }
        }

        private async Task<bool> BanPunishment(Punishment punishment, ulong channelId, SocketGuildUser user)
        {
            var botHierarchy = _client.GetGuild(user.Guild.Id)?.CurrentUser.Hierarchy ?? 0;

            if (botHierarchy > user.Hierarchy)
            {
                await user.Guild.AddBanAsync(user);

                Logger.LogInformation($"Moderation Module: Banning user '{user.Username}'." +
                        $"Guild Id:{user.Guild.Id}, User Id: {user.Id}.");

                var expireTime = punishment.PunishDuration == null ? "Never" : punishment.PunishDuration.ToString();

                await _messageService.Post(channelId, $"Banning user <@{user.Id}>" +
                    $"\nReason: {punishment.WarningType}" +
                    $"Expires: {expireTime}");

                return true;
            }
            else
            {
                Logger.LogInformation($"Moderation Module: Failed to ban user '{user.Username}' as bot has insufficient permissions." +
                        $"Guild Id:{user.Guild.Id}, User Id: {user.Id}.");

                await _messageService.Post(channelId, $"Failed to ban user <@{user.Id}> as bot has insufficient permissions.");

                return false;
            }  
        }

        private async Task AddActivePunishments(ModerationSettings moderationSettings, List<Punishment> punishments, SocketGuildUser user, UserWarnings userData)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var activePunishmentsService = scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

                var userWarningsService = scope.ServiceProvider.GetRequiredService<IEntityService<UserWarnings>>();

                var activePunishments = new List<ActivePunishment>();

                foreach (var punishment in punishments)
                {
                    // Null means apply permanently/punishment is just a kick, so no point in adding punishment to db.
                    if (punishment.PunishType == PunishType.Kick)
                        continue;

                    var expireDate = DateTimeOffset.Now.AddMinutes(punishment.PunishDuration.Value);

                    // Refetching prevents self referencing loop AND ensures the entity is tracked, therefore stopping ef core from trying to re-add it.
                    var userDbEntry = await userWarningsService.Find(userData.Id);

                    // value of 0 means no expiration.
                    if (punishment.PunishDuration == 0)
                        expireDate = DateTimeOffset.MaxValue;

                    activePunishments.Add(new ActivePunishment
                    {
                        PunishmentExpires = expireDate,
                        PunishType = punishment.PunishType,
                        PunishmentId = punishment.Id,
                        RoleId = punishment.RoleId,
                        User = userDbEntry
                    });
                }

                var removePunishmentService = scope.ServiceProvider.GetRequiredService<RemovePunishmentJob>();

                await activePunishmentsService.CreateBulk(activePunishments);

                // Schedule punishment removals where punishments expire. No point in scheduling punishments removal. which enver expire.
                await removePunishmentService.SchedulePunishmentRemovals(activePunishments.Where(x => x.PunishmentExpires > DateTimeOffset.Now));
            }
        }

        private bool IsPunishmentAlreadyActive(Punishment punishment, UserWarnings userData)
        {
            var currentlyActivePunishments = userData.ActivePunishments;

            // bool indicating whether user already has punishment.
            return ( currentlyActivePunishments.Any(x => x.PunishmentId == punishment.Id) );
        }
    }
}