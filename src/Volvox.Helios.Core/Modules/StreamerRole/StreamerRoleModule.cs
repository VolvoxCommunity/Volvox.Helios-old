using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.StreamerRole
{
    /// <summary>
    /// Assign a user the streaming role.
    /// </summary>
    public class StreamerRoleModule : Module
    {
        /// <summary>
        /// Assign a user the streaming role.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        public StreamerRoleModule(IDiscordSettings discordSettings) : base(discordSettings)
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
                    // Get the streaming role.
                    var streamingRole = guildUser.Guild.Roles.FirstOrDefault(r => r.Name == "Streaming");
                    
                    // Add user to role.
                    if (guildUser.Game != null && guildUser.Game.Value.StreamType == StreamType.Twitch)
                    {
                        await AddUserToStreamingRole(guildUser, streamingRole);
                    }

                    // Remove user from role.
                    else if (guildUser.Roles.Any(r => r == streamingRole))
                    {
                        await RemoveUserFromStreamingRole(guildUser, streamingRole);
                    }
                }
            };
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Add the specified used to the specified streaming role.
        /// </summary>
        /// <param name="guildUser">User to add to role.</param>
        /// <param name="streamingRole">Role to add the user to.</param>
        private static async Task AddUserToStreamingRole(IGuildUser guildUser, IRole streamingRole)
        {
            await guildUser.AddRoleAsync(streamingRole);

            Console.WriteLine($"Adding {guildUser.Username}");
        }
        
        /// <summary>
        /// Remove the specified used to the specified streaming role.
        /// </summary>
        /// <param name="guildUser">User to add to role.</param>
        /// <param name="streamingRole">Role to add the user to.</param>
        private static async Task RemoveUserFromStreamingRole(IGuildUser guildUser, IRole streamingRole)
        {
            await guildUser.RemoveRoleAsync(streamingRole);

            Console.WriteLine($"Removing {guildUser.Username}");
        }
    }
}