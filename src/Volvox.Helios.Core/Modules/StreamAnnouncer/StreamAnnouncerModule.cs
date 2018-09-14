using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.StreamAnnouncer
{
    /// <summary>
    ///     Announce the user to a specified channel when the user starts streaming.
    /// </summary>
    public class StreamAnnouncerModule : Module
    {
        private readonly IModuleSettingsService<StreamAnnouncerSettings> _settingsService;
        private IDictionary<ulong, HashSet<StreamAnnouncerMessage>> StreamingList { get; } = new Dictionary<ulong, HashSet<StreamAnnouncerMessage>>();

        /// <summary>
        ///     Announce the user to a specified channel when streaming.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="config">Application configuration.</param>
        /// <param name="settingsService">Settings service.</param>
        public StreamAnnouncerModule(IDiscordSettings discordSettings, ILogger<StreamAnnouncerModule> logger,
            IConfiguration config, IModuleSettingsService<StreamAnnouncerSettings> settingsService) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        ///     Initialize the module on GuildMemberUpdated event.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
        public override Task Init(DiscordSocketClient client)
        {
            // Subscribe to the GuildMemberUpdated event.
            client.GuildMemberUpdated += async (user, guildUser) =>
            {
                var settings = await _settingsService.GetSettingsByGuild(guildUser.Guild.Id, x => x.ChannelSettings);

                if (settings != null && settings.Enabled && settings.ChannelSettings != null)
                    await CheckUser(guildUser, settings.ChannelSettings);
            };

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Announces the user if it's appropriate to do so.
        /// </summary>
        /// <param name="user">User to be evaluated/adjusted for streaming announcement.</param>
        /// <param name="channels">List of channels with module enabled</param>
        private async Task CheckUser(SocketGuildUser user, List<StreamAnnouncerChannelSettings> channels)
        {
            // Add initial hash set for the guild.
            if (!StreamingList.TryGetValue(user.Guild.Id, out var set))
            {
                set = new HashSet<StreamAnnouncerMessage>();
                StreamingList[user.Guild.Id] = set;
            }

            // Check to make sure the user is streaming and not in the streaming list.
            if (user.Game != null && user.Game.Value.StreamType == StreamType.Twitch &&
                !StreamingList.Any(u => u.Key == user.Guild.Id && u.Value.Any(x => x.UserId == user.Id)))
            {
                await AnnounceUserHandler(user, channels);
            }

            // User is not streaming.
            else if (user.Game == null || user.Game.Value.StreamType != StreamType.Twitch)
            {
                // Get user from streaming list.
                var userDataFromList = StreamingList[user.Guild.Id].Where(x => x.UserId == user.Id).ToList();

                // Handle announced streaming messages.
                await AnnouncedMessageHandler(user, userDataFromList, channels);

                // Remove messages from hashset.
                foreach (var m in userDataFromList)
                    StreamingList[user.Guild.Id].Remove(m);       
            }
        }

        /// <summary>
        ///     Handles announcement of a user to specified channels.
        /// </summary>
        /// <param name="user">user to be announced.</param>
        /// <param name="channels">List of channels with module enabled.</param>
        /// <returns></returns>
        private async Task AnnounceUserHandler(SocketGuildUser user, List<StreamAnnouncerChannelSettings> channels)
        {
            var announcements = new List<Task>();

            // Announce to all enabled channels in guild and store message in list.
            foreach (var c in channels)
            {
                var message = new StreamAnnouncerMessage() { UserId = user.Id, ChannelId = c.ChannelId };
                StreamingList[user.Guild.Id].Add(message);
                announcements.Add(AnnounceUser(user, message, c.ChannelId));
            }

            await Task.WhenAll(announcements.ToArray());
        }

        /// <summary>
        ///     Announces the users stream to channel.
        /// </summary>
        /// <param name="user">User to be announecd</param>
        /// <param name="m">Message from StreamingList, this is so it's MessageId can be set.</param>
        /// <param name="channelSettings">Settings of channel the message will be to be announced to.</param>
        /// <returns></returns>
        private async Task AnnounceUser(SocketGuildUser user, StreamAnnouncerMessage m, ulong channelId)
        {
            // Build the embedded message.
            var embed = new EmbedBuilder()
                .WithTitle($"{user.Username} is now live!")
                .WithDescription($"{user.Game?.StreamUrl} - {user.Mention}")
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl(user.GetAvatarUrl())
                .AddInlineField("Title", user.Game?.Name).Build();         

            // Announce the user to the channel specified in settings.
            var messageData = await user.Guild.GetTextChannel(channelId)
                .SendMessageAsync("", embed: embed);

            var messageId = messageData.Id;

            // Sets MessageId in hashset, as hashset holds reference to the message param.
            m.MessageId = messageId;

            Logger.LogDebug($"StreamAnnouncer Module: Announcing user {user.Username}" +
                $" (ID: {m.UserId}) to channel {channelId}. " +
                $" (message ID: {messageId}).");       
        }

        /// <summary>
        ///     Handles the announced messages for a given user.
        /// </summary>
        /// <param name="user">User whom stopped streaming.</param>
        /// <param name="messages">List of messages associated the said user.</param>
        /// <param name="channels">List of channels with module enabled.</param>
        /// <returns></returns>
        private async Task AnnouncedMessageHandler(SocketGuildUser user, List<StreamAnnouncerMessage> messages, List<StreamAnnouncerChannelSettings> channels)
        {
            var messageDeletions = new List<Task>();

            // Delete message from channels where RemoveMessages is true.
            foreach (var m in messages)
            {
                var channel = channels.FirstOrDefault(x => x.ChannelId == m.ChannelId);

                if (!channel.RemoveMessage || channel.ChannelId == 0)
                    continue;

                messageDeletions.Add(DeleteMessageAsync(user, m));
            }
            await Task.WhenAll(messageDeletions.ToArray());
        }

        /// <summary>
        ///     Remove announcement message from channel if necessary.
        /// </summary>
        /// <param name="user">User whom stopped streaming</param>
        /// <param name="m">Message to delete </param>
        private async Task DeleteMessageAsync(SocketGuildUser user, StreamAnnouncerMessage m)
        {
            Logger.LogDebug($"StreamAnnouncer Module: Deleting streaming message {m.MessageId} " +
                $"from {user.Username} (ID: {m.UserId}), " +
                $"on channel {m.ChannelId}.");

            // Convert to array to work with DeleteMessagesAsync.
            var messageIds = new[] { m.MessageId };

            // Delete message.
            await user.Guild.GetTextChannel(m.ChannelId).DeleteMessagesAsync(messageIds);           
        }
    }
}