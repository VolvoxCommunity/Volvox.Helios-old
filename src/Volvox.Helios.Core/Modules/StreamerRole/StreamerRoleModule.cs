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
                    var role = guildUser.Guild.Roles.FirstOrDefault(r => r.Name == "Streaming");
                    
                    // Add user to role.
                    if (guildUser.Game != null && guildUser.Game.Value.StreamType == StreamType.Twitch)
                    {
                        await guildUser.AddRoleAsync(role);
                        
                        Console.WriteLine($"Adding {guildUser.Username}");
                    }

                    // Remove user from role.
                    else if (guildUser.Roles.Any(r => r == role))
                    {
                        await guildUser.RemoveRoleAsync(role);
                        
                        Console.WriteLine($"Removing {guildUser.Username}");
                    }
                }
            };
            
            return Task.CompletedTask;
        }

        public override async Task Execute(DiscordSocketClient client)
        {
            throw new NotImplementedException();
        }
    }
}