using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.NowStreamingModule
{
    /// <summary>
    /// Assign a user the streaming role.
    /// </summary>
    public class NowStreamingModule : Module
    {
        /// <summary>
        /// Assign a user the streaming role.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        public NowStreamingModule(IDiscordSettings discordSettings, ILogger<NowStreamingModule> logger) : base(discordSettings, logger)
        {
        }

        /// <summary>
        /// Initialize the module on GuildMemberUpdated event.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        public override Task Init(DiscordSocketClient client)
        {
            // Subscribe to the GuildMemberUpdated event.
            client.GuildMemberUpdated += async (user, guildUser) =>
            {
                if (IsEnabled)
                {
                    if (guildUser.Game != null && guildUser.Game.Value.StreamType == StreamType.Twitch)
                    {
                        await this.AnnounceUser(guildUser);
                    }
                }
            };
            
            return Task.CompletedTask;
        }

        private async Task AnnounceUser(IGuildUser guildUser)
        {
            
        }
    }
}