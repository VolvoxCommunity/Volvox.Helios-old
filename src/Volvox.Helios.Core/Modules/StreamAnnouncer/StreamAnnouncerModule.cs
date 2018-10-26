using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.StreamAnnouncer
{
    /// <summary>
    ///     Announce the user to a specified channel when the user starts streaming.
    /// </summary>
    public class StreamAnnouncerModule : Module
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IModuleSettingsService<StreamAnnouncerSettings> _settingsService;

        /// <summary>
        ///     Announce the user to a specified channel when streaming.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="config">Application configuration.</param>
        /// <param name="settingsService">Settings service.</param>
        /// <param name="scopeFactory">Scope factory.</param>
        public StreamAnnouncerModule(IDiscordSettings discordSettings, ILogger<StreamAnnouncerModule> logger,
            IConfiguration config, IModuleSettingsService<StreamAnnouncerSettings> settingsService,
            IServiceScopeFactory scopeFactory
        ) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;

            _scopeFactory = scopeFactory;
        }

        private IDictionary<ulong, HashSet<StreamAnnouncerMessage>> _streamingList { get; } =
            new Dictionary<ulong, HashSet<StreamAnnouncerMessage>>();

        /// <summary>
        ///     Initialize the module on GuildMemberUpdated event.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
        public override async Task Init(DiscordSocketClient client)
        {
            // Populate streaming list from database. This is incase the bot restarts, the messages won't be lost.
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageService = scope.ServiceProvider.GetRequiredService<IEntityService<StreamAnnouncerMessage>>();

                var messages = await messageService.GetAll(x => x.StreamAnnouncerSettings);

                foreach (var m in messages)
                {
                    if (!_streamingList.ContainsKey(m.StreamAnnouncerSettings.GuildId))
                        _streamingList.Add(m.StreamAnnouncerSettings.GuildId, new HashSet<StreamAnnouncerMessage>());

                    _streamingList[m.StreamAnnouncerSettings.GuildId].Add(m);
                }
            }

            // Subscribe to the GuildMemberUpdated event.
            client.GuildMemberUpdated += async (user, guildUser) =>
            {
                var settings = await _settingsService.GetSettingsByGuild(guildUser.Guild.Id, x => x.ChannelSettings);

                if (settings != null && settings.Enabled && settings.ChannelSettings != null)
                    await CheckUser(guildUser, settings, settings.ChannelSettings);
            };
        }

        /// <summary>
        ///     Announces the user if it's appropriate to do so.
        /// </summary>
        /// <param name="user">User to be evaluated/adjusted for streaming announcement.</param>
        /// <param name="channels">List of channels with module enabled</param>
        private async Task CheckUser(SocketGuildUser user, StreamAnnouncerSettings settings,
            List<StreamAnnouncerChannelSettings> channels)
        {
            // Add initial hash set for the guild.
            if (!_streamingList.TryGetValue(user.Guild.Id, out var set))
            {
                set = new HashSet<StreamAnnouncerMessage>();
                _streamingList[user.Guild.Id] = set;
            }

            // Check to make sure the user is streaming and not in the streaming list.
            if (user.Game != null && user.Game.Value.StreamType == StreamType.Twitch &&
                !_streamingList.Any(u => u.Key == user.Guild.Id && u.Value.Any(x => x.UserId == user.Id)))
                await AnnounceUserHandler(user, channels);

            // User is not streaming.
            else if (user.Game == null || user.Game.Value.StreamType != StreamType.Twitch)
            {
                // Get user from streaming list.
                var userDataFromList = _streamingList[user.Guild.Id].Where(x => x.UserId == user.Id).ToList();

                // Handle announced streaming messages.
                await AnnouncedMessageHandler(user, userDataFromList, channels);

                // Remove messages from hashset.
                foreach (var m in userDataFromList)
                    _streamingList[user.Guild.Id].Remove(m);
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
            var announcements = new List<Task<StreamAnnouncerMessage>>();

            // Announce to all enabled channels in guild and store message in list.
            foreach (var c in channels)
            {
                var message = new StreamAnnouncerMessage
                    {UserId = user.Id, ChannelId = c.ChannelId};
                _streamingList[user.Guild.Id].Add(message);
                announcements.Add(AnnounceUser(user, message, c.ChannelId));
            }

            var messages = await Task.WhenAll(announcements);

            using (var scope = _scopeFactory.CreateScope())
            {
                var messageService = scope.ServiceProvider.GetRequiredService<IEntityService<StreamAnnouncerMessage>>();

                await messageService.CreateBulk(messages);
            }
        }

        /// <summary>
        ///     Announces the users stream to channel.
        /// </summary>
        /// <param name="user">User to be announced</param>
        /// <param name="m">Message from streaming list, this is so it's MessageId can be set.</param>
        /// <param name="channelId">Id of the channel to announce to.</param>
        /// <returns></returns>
        private async Task<StreamAnnouncerMessage> AnnounceUser(SocketGuildUser user, StreamAnnouncerMessage m,
            ulong channelId)
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

            // Sets navigation property/foreign key.
            m.GuildId = user.Guild.Id;

            Logger.LogDebug($"StreamAnnouncer Module: Announcing user {user.Username}" +
                            $" (ID: {m.UserId}) to channel {channelId}. " +
                            $" (message ID: {messageId}).");

            return m;
        }

        /// <summary>
        ///     Handles the announced messages for a given user.
        /// </summary>
        /// <param name="user">User whom stopped streaming.</param>
        /// <param name="messages">List of messages associated the said user.</param>
        /// <param name="channels">List of channels with module enabled.</param>
        /// <returns></returns>
        private async Task AnnouncedMessageHandler(SocketGuildUser user, List<StreamAnnouncerMessage> messages,
            List<StreamAnnouncerChannelSettings> channels)
        {
            var messageDeletions = new List<Task>();

            // Delete message from channels where RemoveMessages is true.
            foreach (var m in messages)
            {
                var channel = channels.FirstOrDefault(x => x.ChannelId == m.ChannelId);

                if (channel != null && ( !channel.RemoveMessage || channel.ChannelId == 0 ))
                    continue;

                messageDeletions.Add(DeleteMessageAsync(user, m));
            }

            await Task.WhenAll(messageDeletions);

            // Remove streaming messages from database.
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageService = scope.ServiceProvider.GetRequiredService<IEntityService<StreamAnnouncerMessage>>();

                await messageService.RemoveBulk(messages);
            }
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
            var messageIds = new[] {m.MessageId};

            // Delete message.
            await user.Guild.GetTextChannel(m.ChannelId).DeleteMessagesAsync(messageIds);
        }
    }
}