using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.StreamerRole
{
    /// <summary>
    ///     Assign a user the streaming role.
    /// </summary>
    public class StreamerRoleModule : Module
    {
        private readonly IModuleSettingsService<StreamerRoleSettings> _settingsService;

        /// <summary>
        ///     Assign a user the streaming role.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="config">Application configuration.</param>
        /// <param name="settingsService">Settings service.</param>
        public StreamerRoleModule(IDiscordSettings discordSettings, ILogger<StreamerRoleModule> logger,
            IConfiguration config, IModuleSettingsService<StreamerRoleSettings> settingsService) : base(discordSettings,
            logger, config)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        ///     Initialize the module on GuildMemberUpdated event.
        /// </summary>
        /// <param name="client">Client that the module will be registered to.</param>
        public override Task Init(DiscordSocketClient client)
        {
            client.GuildMemberUpdated += async (user, guildUser) =>
            {
                var settings = await _settingsService.GetSettingsByGuild(guildUser.Guild.Id);

                if (settings != null && settings.Enabled)
                {
                    // Get the streaming role.
                    var streamingRole = guildUser.Guild.Roles.FirstOrDefault(r => r.Id == settings.RoleId);

                    // Remove the streaming role if it does not exist.
                    if (streamingRole == null)
                    {
                        await _settingsService.RemoveSetting(settings);

                        Logger.LogError("StreamingRole Module: Role could not be found!");
                    }
                    else
                    {
                        // Add use to role.
                        if (guildUser.Game != null && guildUser.Game.Value.StreamType == StreamType.Twitch)
                            await AddUserToStreamingRole(guildUser, streamingRole);
                        // Remove user from role.
                        else if (guildUser.Roles.Any(r => r == streamingRole))
                            await RemoveUserFromStreamingRole(guildUser, streamingRole);
                    }
                }
            };

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Add the specified user to the specified streaming role.
        /// </summary>
        /// <param name="guildUser">User to add to role.</param>
        /// <param name="streamingRole">Role to add the user to.</param>
        private async Task AddUserToStreamingRole(IGuildUser guildUser, IRole streamingRole)
        {
            await guildUser.AddRoleAsync(streamingRole);

            Logger.LogDebug($"StreamingRole Module: Adding {guildUser.Username} to role {streamingRole.Name}");
        }

        /// <summary>
        ///     Remove the specified user from the specified streaming role.
        /// </summary>
        /// <param name="guildUser">User to remove the role from.</param>
        /// <param name="streamingRole">Role to remove the user from.</param>
        private async Task RemoveUserFromStreamingRole(IGuildUser guildUser, IRole streamingRole)
        {
            await guildUser.RemoveRoleAsync(streamingRole);

            Logger.LogDebug($"StreamingRole Module: Removing {guildUser.Username} from role {streamingRole.Name}");
        }
    }
}