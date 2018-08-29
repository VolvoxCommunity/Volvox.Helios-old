using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Volvox.Helios.Domain.Discord;
using Volvox.Helios.Service.Clients;

namespace Volvox.Helios.Service.Discord.Guild
{
    public class DiscordGuildService : IDiscordGuildService
    {
        private readonly DiscordAPIClient _client;

        public DiscordGuildService(DiscordAPIClient client)
        {
            _client = client;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Get all of the channels for the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>List of channels in the guild.</returns>
        public async Task<List<Channel>> GetChannels(ulong guildId)
        {
            var channels = await _client.GetGuildChannels(guildId);

            return JsonConvert.DeserializeObject<List<Channel>>(channels);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Get all of the roles in the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>List of roles in the guild.</returns>
        public async Task<List<Role>> GetRoles(ulong guildId)
        {
            var roles = await _client.GetGuildRoles(guildId);

            return JsonConvert.DeserializeObject<List<Role>>(roles);
        }
    }
}