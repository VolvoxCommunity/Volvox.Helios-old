using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.DadModule
{
    /// <summary>
    /// The ultimate in discord bots - the Dad Bot
    /// </summary>
    public class DadModule : Module
    {
        private const string DadName = "Volvox.Helios";
        private const string MatchPattern = "^[iI]'*[mM]\\s+";
        private static readonly Regex _pattern =
            new Regex(MatchPattern);

        private readonly IModuleSettingsService<DadModuleSettings> _moduleSettings;

        public DadModule(IDiscordSettings discordSettings,
            ILogger<IModule> logger,
            IConfiguration config,
            IModuleSettingsService<DadModuleSettings> moduleSettings)
            : base(discordSettings, logger, config)
        {
            _moduleSettings = moduleSettings;
        }

        public override Task Init(DiscordSocketClient client)
        {
            client.MessageReceived += OnMessageReceived;
            client.MessageUpdated += OnMessageUpdated;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async override Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var settings = await _moduleSettings.GetSettingsByGuild(guildId);
            return settings?.Enabled ?? false;
        }

        /// <summary>
        /// Callback that receives new messages on a server.
        /// </summary>
        /// <param name="socketMessage"></param>
        /// <returns></returns>
        private Task OnMessageReceived(SocketMessage socketMessage)
        {
            return TryPostMessage(socketMessage);
        }

        /// <summary>
        /// Callback that receives message updates on a server.
        /// </summary>
        /// <param name="messageCache"></param>
        /// <param name="socketMessage"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        private Task OnMessageUpdated(Cacheable<IMessage, ulong> messageCache,
            SocketMessage socketMessage,
            ISocketMessageChannel channel)
        {
            return TryPostMessage(socketMessage);
        }

        /// <summary>
        /// Will attempt to post the quintessential dad response if the incoming message warrents it. 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task TryPostMessage(SocketMessage message)
        {
            if (message.Source != MessageSource.User)
                return;

            if (message.Channel is SocketTextChannel textChannel)
            {
                var isEnabled = await IsEnabledForGuild(textChannel.Guild.Id);

                if (!isEnabled)
                    return;

                if (TryParseMessage(message, out var outgoing))
                {
                    var formatted = $"Hi {outgoing}, I'm {DadName}.";
                    await message.Channel.SendMessageAsync(formatted);
                }
            }
        }

        /// <summary>
        /// Parses out the "I'm" portion of a string if it contains it and returns the rest.
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="outgoing"></param>
        /// <returns></returns>
        private bool TryParseMessage(SocketMessage incoming, out string outgoing)
        {
            if(_pattern.IsMatch(incoming.Content))
            {
                var parts = _pattern.Split(incoming.Content);
                if (parts.Length > 1)
                {
                    var sanitizedText = parts[1]
                        .Replace("@", "")
                        .Trim();

                    outgoing = sanitizedText;
                    return true;
                }
            }

            outgoing = null;
            return false;
        }
    }
}
