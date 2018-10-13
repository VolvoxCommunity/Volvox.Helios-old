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

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        #region Private vars

        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        private readonly IMessageService _messageService;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly string[] _defaultBannedWords;

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

            // TODO: add default worsd to banned words private var
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

            var author = message.Author;

            // If the user or channel is globally whitelisted, there is no point in checking the message contents.
            if (HasBypassAuthority(author, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Global),
                settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Global)))
                return;

            if (!HasBypassAuthority(author, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Profanity), settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Profanity)) && ProfanityCheck(message, settings.ProfanityFilter))
            {
                await HandleViolation(settings, message, user, WarningType.Profanity);
                return;
            }

            if (!HasBypassAuthority(author, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Link), settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Link)) && LinkCheck(message, settings.LinkFilter))
            {
                await HandleViolation(settings, message, user, WarningType.Link);
                return;
            }
        }

        private bool HasBypassAuthority(SocketUser author, IEnumerable<WhitelistedChannel> whitelistedChannels, IEnumerable<WhitelistedRole> whitelistedRoles) {
            if (author.IsBot) return true;
            return false;
        }

        private bool ProfanityCheck(SocketMessage message, ProfanityFilter profanityFilter)
        {
            // Normalize message to lowercase and split into array of words
            var messageWords = message.Content.ToLower().Split(" ");

            var bannedWords = profanityFilter.BannedWords.Select(w => w.Word).ToList();

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
            var messageWords = message.Content.ToLower().Split(" ");

            var whitelistedLinks = linkFilter.WhitelistedLinks.Select(l => l.Link);

            var urlCheck = new Regex(@"[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)");

            var isLinkLegal = false;

            foreach (var word in messageWords)
            {
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

            using (var scope = _scopeFactory.CreateScope())
            {
                var userWarningService = scope.ServiceProvider.GetRequiredService<IEntityService<UserWarnings>>();

                var listUserData = await userWarningService.Get(x => x.UserId == user.Id, u => u.Warnings);

                var userData = listUserData[0];

                if (userData == null)
                {
                    // TODO: create user entry in db
                }

                // add warning to database
                await AddWarning(scope, moderationSettings, userData, warningType);

                var userWarnings = userData.Warnings.Where(x => x.WarningExpires > DateTimeOffset.Now);

                var specificWarningCount = userWarnings.Count(x => x.WarningType == warningType);

                var allWarningsCount = userWarnings.Count();

                var punishments = new List<Punishment>();

                punishments.AddRange(moderationSettings.Punishments.Where(x => x.WarningType == WarningType.General && x.WarningThreshold == allWarningsCount));

                punishments.AddRange(moderationSettings.Punishments.Where(x => x.WarningType == warningType && x.WarningThreshold == specificWarningCount));

                await ApplyPunishment(punishments);
              
            }

            await _messageService.Post(message.Channel.Id, $"Message by <@{user.Id}> deleted\nReason: {warningType}");
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

        private async Task ApplyPunishment(IEnumerable<Punishment> punishments) {

        }
    }
}
