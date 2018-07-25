using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using System.Collections.Generic;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.StreamAnnouncer
{
    /// <summary>
    /// Announce the user to a specified channel when the user starts streaming.
    /// </summary>
    public class StreamAnnouncerModule : Module
    {
        private readonly IModuleSettingsService<StreamAnnouncerSettings> _settingsService;

        private IDictionary<SocketGuild, SocketGuildUser> StreamingList { get; } = new Dictionary<SocketGuild, SocketGuildUser>();

        /// <summary>
        /// Announce the user to a specified channel when streaming.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="settingsService">Settings serivce.</param>
        public StreamAnnouncerModule(IDiscordSettings discordSettings, ILogger<StreamAnnouncerModule> logger, IModuleSettingsService<StreamAnnouncerSettings> settingsService) : base(discordSettings, logger)
        {
            _settingsService = settingsService;
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
                    await CheckUser(guildUser);
                }
            };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Announces the user if it's appropriate to do so.
        /// </summary>
        /// <param name="user">User to be evaluated/adjusted for streaming announcement.</param>
        private async Task CheckUser(SocketGuildUser user)
        {
            // Check to make sure the user is streaming and not in the streaming list.
            if (user.Game != null && user.Game.Value.StreamType == StreamType.Twitch &&
                !StreamingList.Any(u => u.Key.Id == user.Guild.Id && u.Value.Id == user.Id))
            {
                // Add user to the streaming list.
                StreamingList.Add(user.Guild, user);

                // Announce that the user is streaming.
                await AnnounceUser(user);
            }

            // User is not streaming.
            else
            {
                // Remove user from the streaming list.
                if (StreamingList.Any(u => u.Key.Id == user.Guild.Id && u.Value.Id == user.Id))
                {
                    StreamingList.Remove(StreamingList.Single(u => u.Key.Id == user.Guild.Id && u.Value.Id == user.Id).Key);
                }
            }
        }

        /// <summary>
        /// Announces the users stream to the appropriate channel.
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

            // Get the settings from the database.
            var settings = await _settingsService.GetSettingsByGuild(user.Guild.Id);

            if (settings != null)
            {
                var announceChannelId = settings.AnnouncementChannelId;

                if (announceChannelId != 0)
                {
                    Logger.LogDebug($"StreamAnnouncer Module: Announcing {user.Username}");

                    // Announce the user to the channel specified in settings.
                    await user.Guild.GetTextChannel(announceChannelId)
                        .SendMessageAsync("", embed: embed);
                }
            }
        }
    }
}