using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Modules.StreamAnnouncer;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.Streamer
{
    /// <summary>
    ///     Announce the user to a specified channel when the user starts streaming and assign specified streaming role to the
    ///     user.
    /// </summary>
    public class StreamerModule : Module
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IModuleSettingsService<StreamerSettings> _settingsService;

        /// <summary>
        ///     Announce the user to a specified channel when the user starts streaming and assign specified streaming role to the
        ///     user.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="config">Application configuration.</param>
        /// <param name="settingsService">Settings service.</param>
        /// <param name="scopeFactory">Scope factory.</param>
        public StreamerModule(IDiscordSettings discordSettings, ILogger<StreamerModule> logger,
            IConfiguration config, IModuleSettingsService<StreamerSettings> settingsService,
            IServiceScopeFactory scopeFactory) : base(
            discordSettings, logger, config)
        {
            _settingsService = settingsService;
            _scopeFactory = scopeFactory;
        }

        private IDictionary<ulong, HashSet<StreamAnnouncerMessage>> StreamingList { get; } =
            new Dictionary<ulong, HashSet<StreamAnnouncerMessage>>();

        /// <summary>
        ///     Returns true if the module is enabled for the specified guild and false if not.
        /// </summary>
        /// <param name="guildId">Id if the guild to check.</param>
        /// <returns>True if the module is enabled for the specified guild and false if not.</returns>
        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var settings = await _settingsService.GetSettingsByGuild(guildId);

            return settings != null && settings.Enabled;
        }

        /// <summary>
        ///     Initialize the module on GuildMemberUpdated event.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
        public override async Task Init(DiscordSocketClient client)
        {
            // Populate streaming list from database. This is in case the bot restarts, the messages won't be lost.
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageService = scope.ServiceProvider.GetRequiredService<IEntityService<StreamAnnouncerMessage>>();

                var messages = await messageService.GetAll(x => x.StreamerSettings);

                foreach (var m in messages)
                {
                    if (!StreamingList.ContainsKey(m.StreamerSettings.GuildId))
                        StreamingList.Add(m.StreamerSettings.GuildId, new HashSet<StreamAnnouncerMessage>());

                    StreamingList[m.StreamerSettings.GuildId].Add(m);
                }
            }

            // Subscribe to the GuildMemberUpdated event.
            client.GuildMemberUpdated += async (user, guildUser) =>
            {
                var settings = await _settingsService.GetSettingsByGuild(guildUser.Guild.Id, x => x.ChannelSettings,
                    x => x.WhiteListedRoleIds);

                if (settings != null && settings.Enabled)
                    try
                    {
                        // Streamer Role
                        if (settings.StreamerRoleEnabled)
                            await HandleStreamerRole(guildUser, settings);

                        // Stream Announcer
                        if (settings.ChannelSettings != null)
                            await CheckUser(guildUser, settings.ChannelSettings, settings);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Streamer Module: Error occurred '{e.Message}'");
                    }
            };
        }

        private async Task HandleStreamerRole(SocketGuildUser guildUser, StreamerSettings settings)
        {
            // Get the streaming role.
            var streamingRole = guildUser.Guild.Roles.FirstOrDefault(r => r.Id == settings.RoleId);

            // Remove the streaming role if it does not exist.
            if (streamingRole == null)
            {
                await _settingsService.RemoveSetting(settings);

                Logger.LogError("Streamer Module: Role could not be found!");
            }
            else
                using (var scope = _scopeFactory.CreateScope())
                {
                    var botService = scope.ServiceProvider.GetRequiredService<IBot>();

                    var botRolePosition = botService.GetBotRoleHierarchy(guildUser.Guild.Id);

                    if (streamingRole.Position < botRolePosition)
                    {
                        // Add use to role.
                        if (guildUser.Activity != null && guildUser.Activity.Type == ActivityType.Streaming &&
                            IsUserWhiteListed(settings, guildUser))
                            await AddUserToStreamingRole(guildUser, streamingRole);

                        // Remove user from role.
                        else if (guildUser.Roles.Any(r => r.Id == streamingRole.Id))
                            await RemoveUserFromStreamingRole(guildUser, streamingRole);
                    }
                    else
                    {
                        Logger.LogError(
                            $"Streamer Module: Could not add/remove role as bot has insufficient hierarchical position. " +
                            $"Guild Id: {guildUser.Guild.Id}");

                        await botService.GetGuild(guildUser.Guild.Id).Owner.SendMessageAsync(
                            $"We couldn't add/remove Role '{streamingRole.Name}' to a user as the role's hierarchical position is greater than the bot's.{Environment.NewLine}" +
                            $"To fix this, please adjust your role's position and then re-enable the module via our website.{Environment.NewLine}" +
                            $"Guild: {guildUser.Guild.Name}");

                        var settingsDb = await _settingsService.GetSettingsByGuild(guildUser.Guild.Id);

                        settingsDb.StreamerRoleEnabled = false;

                        await _settingsService.SaveSettings(settingsDb);
                    }
                }
        }

        /// <summary>
        ///     Announces the user if it's appropriate to do so.
        /// </summary>
        /// <param name="user">User to be evaluated/adjusted for streaming announcement.</param>
        /// <param name="channels">List of channels with module enabled</param>
        /// <param name="settings">Streamer settings for specified guild.</param>
        private async Task CheckUser(SocketGuildUser user, List<StreamerChannelSettings> channels,
            StreamerSettings settings)
        {
            // Add initial hash set for the guild.
            if (!StreamingList.TryGetValue(user.Guild.Id, out var set))
            {
                set = new HashSet<StreamAnnouncerMessage>();
                StreamingList[user.Guild.Id] = set;
            }

            // Check to make sure the user is streaming and not in the streaming list.
            if (user.Activity != null && user.Activity.Type == ActivityType.Streaming)
            {
                // If the user is not in the streaming list, they just started streaming. So, handle announcement.
                if (!StreamingList.Any(u => u.Key == user.Guild.Id && u.Value.Any(x => x.UserId == user.Id)) &&
                    IsUserWhiteListed(settings, user))
                    await AnnounceUserHandler(user, channels);

                // Else, the user is already streaming and already has an announcement message.
                // This happens when GuildMemberUpdated is triggered by something other than the user stopping their stream.
                // So, do nothing.
            }
            // User is not streaming.
            else
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
        private async Task AnnounceUserHandler(SocketGuildUser user, List<StreamerChannelSettings> channels)
        {
            try
            {
                var announcements = new List<Task<StreamAnnouncerMessage>>();

                // Announce to all enabled channels in guild and store message in list.
                foreach (var c in channels)
                {
                    var message = new StreamAnnouncerMessage
                    {
                        UserId = user.Id,
                        ChannelId = c.ChannelId
                    };

                    StreamingList[user.Guild.Id].Add(message);
                    announcements.Add(AnnounceUser(user, message, c.ChannelId));
                }

                var messages = await Task.WhenAll(announcements);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var messageService =
                        scope.ServiceProvider.GetRequiredService<IEntityService<StreamAnnouncerMessage>>();

                    await messageService.CreateBulk(messages);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Streamer Module[AnnounceUserHandler]: Error occurred '{e.Message}'. Guild ID: {user.Guild.Id}, Channels: {channels}, User ID: {user.Id}.");
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
            var streamingGame = (StreamingGame)user.Activity;

            // Build the embedded message.
            var embed = new EmbedBuilder()
                .WithTitle($"{user.Username} is now live!")
                .WithDescription($"{streamingGame.Url} - {user.Mention}")
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl(user.GetAvatarUrl())
                .AddField("Title", user.Activity.Name, true).Build();

            // Announce the user to the channel specified in settings.
            var messageData = await user.Guild.GetTextChannel(channelId)
                .SendMessageAsync("", embed: embed);

            var messageId = messageData.Id;

            // Sets MessageId in hashset, as hashset holds reference to the message param.
            m.MessageId = messageId;

            // Sets navigation property/foreign key.
            m.GuildId = user.Guild.Id;

            Logger.LogDebug($"Streamer Module: Announcing user {user.Username}. Guild ID: {m.GuildId}, " +
                            $"Channel ID: {m.ChannelId}, User ID: {m.UserId}, Message ID: {m.MessageId}.");

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
            List<StreamerChannelSettings> channels)
        {
            try
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
                    var messageService =
                        scope.ServiceProvider.GetRequiredService<IEntityService<StreamAnnouncerMessage>>();

                    await messageService.RemoveBulk(messages);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Streamer Module[AnnouncedMessageHandler]: Error occurred '{e.Message}'. Guild ID: {user.Guild.Id}, Channels: {channels}, User ID: {user.Id}, Messages: {messages}.");
            }
        }

        /// <summary>
        ///     Remove announcement message from channel if necessary.
        /// </summary>
        /// <param name="user">User whom stopped streaming</param>
        /// <param name="m">Message to delete </param>
        private async Task DeleteMessageAsync(SocketGuildUser user, StreamAnnouncerMessage m)
        {
            Logger.LogDebug(
                $"Streamer Module: Deleting streaming message from {user.Username}. Guild ID: {m.GuildId}, " +
                $"Channel ID: {m.ChannelId}, User ID: {m.UserId}, Message ID: {m.MessageId}.");

            // Delete message.
            var message = await user.Guild.GetTextChannel(m.ChannelId).GetMessageAsync(m.MessageId);

            await message.DeleteAsync();
        }

        /// <summary>
        ///     Add the specified user to the specified streaming role.
        /// </summary>
        /// <param name="guildUser">User to add to role.</param>
        /// <param name="streamingRole">Role to add the user to.</param>
        private async Task AddUserToStreamingRole(IGuildUser guildUser, IRole streamingRole)
        {
            await guildUser.AddRoleAsync(streamingRole);

            Logger.LogDebug($"Streamer Module: Adding {guildUser.Username} to role {streamingRole.Name}. " +
                            $"Guild ID: {guildUser.GuildId}, User ID: {guildUser.Id}.");
        }

        /// <summary>
        ///     Remove the specified user from the specified streaming role.
        /// </summary>
        /// <param name="guildUser">User to remove the role from.</param>
        /// <param name="streamingRole">Role to remove the user from.</param>
        private async Task RemoveUserFromStreamingRole(IGuildUser guildUser, IRole streamingRole)
        {
            await guildUser.RemoveRoleAsync(streamingRole);

            Logger.LogDebug($"Streamer Module: Removing {guildUser.Username} from role {streamingRole.Name}. " +
                            $"Guild ID: {guildUser.GuildId}, User ID: {guildUser.Id}.");
        }

        /// <summary>
        ///     Checks if a user is part of the white listed roles.
        /// </summary>
        /// <param name="settings">Streamer settings for specified guild.</param>
        /// <param name="guildUser">User to check.</param>
        /// <returns>True if the user is a part of the white listed roles; otherwise false.</returns>
        private static bool IsUserWhiteListed(StreamerSettings settings, SocketGuildUser guildUser)
        {
            var whiteListedRoles = settings.WhiteListedRoleIds.Select(w => w.RoleId).ToList();

            return !whiteListedRoles.Any() || guildUser.Roles.Any(r => whiteListedRoles.Contains(r.Id));
        }
    }
}