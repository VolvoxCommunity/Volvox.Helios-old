using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using System.Collections.Generic;

namespace Volvox.Helios.Core.Modules.StreamAnnouncer
{
    /// <summary>
    /// Announce the user to a specified channel when streaming.
    /// </summary>
    public class StreamAnnouncerModule : Module
    {

        // Dan: Likely want this batched out to be read from a settings file or be adjustable
        // from Web level.
        private const ulong AnnounceChannelId = 392962633495740418;

        // HashSet is optimal for situations where order isn't important and search performance is.
        // https://stackoverflow.com/questions/150750/hashset-vs-list-performance
        private HashSet<ulong> CurrentStreamers { get; } = new HashSet<ulong>();

        /// <summary>
        /// Assign a user the streaming role.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        public StreamAnnouncerModule(IDiscordSettings discordSettings, ILogger<StreamAnnouncerModule> logger) : base(discordSettings, logger)
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
            if (user.Game != null && user.Game.Value.StreamType == StreamType.Twitch && !CurrentStreamers.Contains(user.Id))
            {
                // Add user to the streaming list.
                CurrentStreamers.Add(user.Id);

                // Announce that the user is streaming.
                await AnnounceUser(user);
            }

            // User is not streaming.
            else
            {
                // Remove user from the streaming list.
                CurrentStreamers.Remove(user.Id);
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