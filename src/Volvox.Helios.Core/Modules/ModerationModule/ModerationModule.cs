﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Services.MessageService;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        private readonly IModuleSettingsService<ModerationSettings> _settingsService;
        private readonly IMessageService _messageService;
        private readonly IServiceScopeFactory _scopeFactory;

        public ModerationModule(IDiscordSettings discordSettings, ILogger<ModerationModule> logger,
            IConfiguration config, IModuleSettingsService<ModerationSettings> settingsService, IMessageService messageService,
            IServiceScopeFactory scopeFactory
        ) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;
             
            _messageService = messageService;

            _scopeFactory = scopeFactory;
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
            var author = message.Author;

            var containsProfanity = await ProfanityCheck(message);

            var containsLink = LinkCheck(message);

            if (containsProfanity && !HasBypassAuthority(author))
            {
                await HandleViolation(message, $"Watch your language, {author.Username}!");
                return;
            }

            if (containsLink && !HasBypassAuthority(author))
            {
                await HandleViolation(message, $"You don't have permission to post links, {author.Username}!");
                return;
            }
        }

        private bool HasBypassAuthority(SocketUser author) {
            if (author.IsBot) return true;
            return false;
        }

        private async Task<bool> ProfanityCheck(SocketMessage message)
        {
            var settings = await _settingsService.GetSettingsByGuild(472468560657121280, s => s.ProfanityFilter.BannedWords);

            var profanityFilter = settings.ProfanityFilter;

            profanityFilter.BannedWords.Add(new BannedWord()
            {
                Word = "test",
                ProfanityFilter = profanityFilter
            });

            await _settingsService.SaveSettings(settings);

            var bannedWords = settings.ProfanityFilter.BannedWords;

            foreach (var word in bannedWords)
            {
                if (message.Content == word.Word)
                    return true;
            }
            return false;


        }

        private bool LinkCheck(SocketMessage message)
        {
            var urlCheck = new Regex(@"[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)");
            return urlCheck.IsMatch(message.Content);
        }

        private async Task HandleViolation(SocketMessage message, string warningMessage)
        {
            await message.DeleteAsync();

            await _messageService.Post(message.Channel.Id, warningMessage);
        }
    }
}
