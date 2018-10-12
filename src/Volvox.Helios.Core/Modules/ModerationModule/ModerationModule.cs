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

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        #region Private vars

        private readonly IModuleSettingsService<ModerationSettings> _settingsService;

        private readonly IMessageService _messageService;

        #endregion

        public ModerationModule(IDiscordSettings discordSettings, ILogger<ModerationModule> logger,
            IConfiguration config, IModuleSettingsService<ModerationSettings> settingsService, IMessageService messageService
        ) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;
             
            _messageService = messageService;
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
                s => s.ProfanityFilter.BannedWords, s => s.LinkFilter.WhitelistedLinks, s => s.Punishments, s => s.UserWarnings.Select(u => u.UserId == user.Id), s => s.WhitelistedChannels, s => s.WhitelistedRoles
            );

            var author = message.Author;

            // If the user or channel is globally whitelisted, there is no point in checking the message contents.
            if (HasBypassAuthority(author, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Global),
                settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Global)))
                return;

            if (!HasBypassAuthority(author, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Profanity), settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Profanity)) && ProfanityCheck(message, settings.ProfanityFilter))
            {
                await HandleViolation(message, $"Watch your language, {author.Username}!");
                return;
            }

            if (!HasBypassAuthority(author, settings.WhitelistedChannels.Where(c => c.WhitelistType == WhitelistType.Link), settings.WhitelistedRoles.Where(r => r.WhitelistType == WhitelistType.Link)) && LinkCheck(message, settings.LinkFilter))
            {
                await HandleViolation(message, $"You don't have permission to post links, {author.Username}!");
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

            var bannedWords = profanityFilter.BannedWords.Select(w => w.Word);

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

            foreach (var word in messageWords)
            {
                if (urlCheck.IsMatch(word))
                {
                    foreach (var link in whitelistedLinks)
                    {
                        if (link == word) return true;
                    }
                }
            }       
            return false;
        }

        private async Task HandleViolation(SocketMessage message, string warningMessage)
        {
            await message.DeleteAsync();

            await _messageService.Post(message.Channel.Id, warningMessage);
        }
    }
}
