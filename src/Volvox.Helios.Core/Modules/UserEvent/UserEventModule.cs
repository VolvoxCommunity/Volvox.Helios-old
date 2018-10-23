using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Modules.StreamerRole;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.UserEvent
{
    public class UserEventModule : Module
    {
        private readonly IModuleSettingsService<UserEventSettings> _settingsService;

        /// <summary>
        ///     Perform actions based on a user event.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="config">Application configuration.</param>
        /// <param name="settingsService">Settings service.</param>
        public UserEventModule(IDiscordSettings discordSettings, ILogger<StreamerRoleModule> logger,
            IConfiguration config, IModuleSettingsService<UserEventSettings> settingsService) : base(discordSettings,
            logger, config)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        ///     Initialize the module.
        /// </summary>
        /// <param name="client">Client that the module will be registered to.</param>
        public override Task Init(DiscordSocketClient client)
        {
            client.UserLeft += async user =>
            {
                var settings = await _settingsService.GetSettingsByGuild(user.Guild.Id);

                if (settings.EnableUserLeftEvent) HandleUserLeftEvent(user, settings.UserLeftEventChannelId);
            };

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Handle when a user leaves a guild.
        /// </summary>
        /// <param name="user">User that left.</param>
        /// <param name="announceChannelId">Channel to announce the event to.</param>
        private static async void HandleUserLeftEvent(SocketGuildUser user, ulong announceChannelId)
        {
            var channel = user.Guild.GetTextChannel(announceChannelId);

            await channel.SendMessageAsync($"{user.Username} has left the server.");
        }
    }
}