using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using System.Collections.Generic;

namespace Volvox.Helios.Core.Modules.NowStreaming
{
    /// <summary>
    /// Announce the user to a specified channel when streaming.
    /// </summary>
    public class NowStreamingModule : Module
    {

        // Dan: Likely want this batched out to be read from a settings file or be adjustable
        // from Web level.
        private const ulong AnnounceChannelId = 392962633495740418;

        private List<SocketGuildUser> StreamingList { get; } = new List<SocketGuildUser>();

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
                    await this.UpdateUser(guildUser);
                }
            };
            
            return Task.CompletedTask;
        }

        // Dan: Pre-porting, this had "[RequireUserPermission(GuildPermission.Administrator)]"
        // as an attribute tag- is that required/desired here? Not sure of OG context.

        /// <summary>
        /// Announces the user if it's appropriate to do so.
        /// </summary>
        /// <param name="user">User to be evaluated/adjusted for streaming announcement.</param>
        private async Task UpdateUser(SocketGuildUser user)
        {
            // Check to make sure the user is streaming and not in the streaming list.
            if (user.Game != null && user.Game.Value.StreamType == StreamType.Twitch && StreamingList.All(u => u.Id != user.Id))
            {
                // Add user to the streaming list.
                StreamingList.Add(user);

                // Announce that the user is streaming.
                await AnnounceUser(user);
            }

            // User is not streaming.
            else
            {
                // Remove user from the streaming list.
                if (StreamingList.Any(u => u.Id == user.Id))
                {
                    StreamingList.Remove(StreamingList.Single(u => u.Id == user.Id));
                }
            }
        }

        /// <summary>
        /// Announces the user's stream to the appropriate channel.
        /// </summary>
        /// <param name="user">User to be announced.</param>
        private async Task AnnounceUser(SocketGuildUser user)
        {
            // Build the embedded message.
            var embed = new EmbedBuilder()
                .WithTitle($"{user.Username} is now live!")
                .WithDescription($"{user.Game?.StreamUrl} - {user.Mention}")
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl(user.GetAvatarUrl())
                .AddInlineField("Title", user.Game?.Name).Build();

            // Send the message to the streaming now channel.
            await user.Guild.GetTextChannel(AnnounceChannelId)
                .SendMessageAsync("", embed: embed);
        }
    }
}