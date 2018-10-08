using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Services.MessageService;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule
{
    public class ModerationModule : Module
    {
        private readonly IModuleSettingsService<ModerationSettings> _settingsService;
        private readonly IMessageService _messageService;

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
            var author = message.Author;

            var containsProfanity = ProfanityCheck(message);

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

        private bool ProfanityCheck(SocketMessage message)
        {
            return true;
        }

        private bool LinkCheck(SocketMessage message)
        {
            return false;
        }

        private async Task HandleViolation(SocketMessage message, string warningMessage)
        {
            await message.DeleteAsync();

            await _messageService.Post(message.Channel.Id, warningMessage);
        }
    }
}
