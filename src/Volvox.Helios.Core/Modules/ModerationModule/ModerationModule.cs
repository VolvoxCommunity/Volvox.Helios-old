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
using System.IO;
using Newtonsoft.Json.Linq;
using Discord;

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        #region Private vars

        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        private readonly IMessageService _messageService;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly List<string> _defaultBannedWords = new List<string>();

        #endregion

        public ModerationModule(IDiscordSettings discordSettings, ILogger<ModerationModule> logger,
            IConfiguration config, IModuleSettingsService<ModerationSettings> settingsService,
            IMessageService messageService, IServiceScopeFactory scopeFactory
        ) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;
             
            _messageService = messageService;

            _scopeFactory = scopeFactory;

            var defaultBannedWords = config.GetSection("BannedWords").GetChildren();

            foreach (var word in defaultBannedWords)
                _defaultBannedWords.Add(word.Value);
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

            return Task.CompletedTask;
        }

        private async Task CheckMessage(SocketMessage message)
        {
            var user = message.Author as SocketGuildUser;

            var settings = await _settingsService.GetSettingsByGuild(user.Guild.Id,
                s => s.ProfanityFilter.BannedWords, s => s.LinkFilter.WhitelistedLinks, s => s.Punishments, s => s.WhitelistedChannels, s => s.WhitelistedRoles  
            );

            var channelPostedId = message.Channel.Id;

            var authorRoles = user.Roles;

            // If the user or channel is globally whitelisted, there is no point in checking the message contents.
            if (HasBypassAuthority(user, channelPostedId, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Global),
                settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Global)))
                return;

            if (!HasBypassAuthority(user, channelPostedId, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Profanity),
                settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Profanity)) && ProfanityCheck(message, settings.ProfanityFilter))
            {
                await HandleViolation(settings, message, user, WarningType.Profanity);
                return;
            }

            if (!HasBypassAuthority(user, channelPostedId, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Link),
                settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Link)) && LinkCheck(message, settings.LinkFilter))
            {
                await HandleViolation(settings, message, user, WarningType.Link);
                return;
            }
        }

        private bool HasBypassAuthority(SocketGuildUser user, ulong postedChannelId,
            IEnumerable<WhitelistedChannel> whitelistedChannels, IEnumerable<WhitelistedRole> whitelistedRoles) {
            // Bots bypass check.
            if (user.IsBot) return true;

            // Check if channel id is whitelisted.
            if (whitelistedChannels.Any(x => x.ChannelId == postedChannelId)) return true;

            // Check if role is whitelisted.
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

            using (var scope = _scopeFactory.CreateScope())
            {
                var userWarningService = scope.ServiceProvider.GetRequiredService<IEntityService<UserWarnings>>();

                var listUserData = await userWarningService.Get(x => x.UserId == user.Id, u => u.Warnings);

                var userData = listUserData[0];

                // User isn't tracked yet, so create new entry for them.
                if (userData == null)
                {
                    await userWarningService.Create(new UserWarnings()
                    {
                        ModerationSettings = moderationSettings,
                        UserId = user.Id
                    });
                }

                // Add warning to database.
                await AddWarning(scope, moderationSettings, userData, warningType);

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

                await ApplyPunishment(scope, moderationSettings, punishments, user);
              
            }
        }
 
        private async Task AddWarning(IServiceScope scope, ModerationSettings moderationSettings, UserWarnings user, WarningType warningType)
        {
            var warningService = scope.ServiceProvider.GetRequiredService<IEntityService<Warning>>();

            var specificWarningDuration = GetWarningDuration(moderationSettings, warningType);

            var warning = new Warning()
            {
                User = user,
                WarningRecieved = DateTimeOffset.Now,
                WarningExpires = DateTimeOffset.Now.AddMinutes(specificWarningDuration),
                WarningType = warningType
            };

            await warningService.Create(warning);

            return;
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

        private async Task ApplyPunishment(IServiceScope scope, ModerationSettings moderationSettings, IEnumerable<Punishment> punishments, SocketGuildUser user)
        {
            var appliedPunishments = new List<Punishment>();

            foreach (var punishment in punishments)
            {        
                switch (punishment.PunishType)
                {
                    case ( PunishType.Kick ):
                        await AddRolePunishment(punishment, user);
                        break;

                    case ( PunishType.Ban ):
                        await AddRolePunishment(punishment, user);
                        break;

                    case ( PunishType.AddRole ):
                        await AddRolePunishment(punishment, user);
                        break;
                }

                await AddActivePunishments(scope, moderationSettings, punishments, user);
            }       
        }

        private async Task AddRolePunishment(Punishment punishment, SocketGuildUser user)
        {
            if (!punishment.RoleId.HasValue) return;

            var guild = user.Guild;

            var role = guild.GetRole(punishment.RoleId.Value);

            await user.AddRoleAsync(role);
        }

        private async Task KickPunishment(Punishment punishment, SocketGuildUser user)
        {
            await user.KickAsync();
        }

        private async Task BanPunishment(Punishment punishment, SocketGuildUser user)
        {
            await user.Guild.AddBanAsync(user);
        }

        private async Task AddActivePunishments(IServiceScope scope, ModerationSettings moderationSettings, IEnumerable<Punishment> punishments, SocketGuildUser user)
        {
            var activePunishmentsService = scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

            var activePunishments = new List<ActivePunishment>();

            var modset = await _settingsService.GetSettingsByGuild(user.Guild.Id);

            foreach (var punishment in punishments)
            {
                // Null means apply permanently/punishment is just a kick, so no adding punishment to db.
                if (punishment.PunishDuration == null) continue;

                activePunishments.Add(new ActivePunishment
                {
                    GuildId = moderationSettings.GuildId,
                    PunishmentExpires = DateTimeOffset.Now.AddMinutes(punishment.PunishDuration.Value),
                    PunishType = punishment.PunishType,
                    RoleId = punishment.RoleId,
                    UserId = user.Id
                });
            }

            await activePunishmentsService.CreateBulk(activePunishments);
        }
    }
}
