using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Volvox.Helios.Domain.Discord;
using Volvox.Helios.Service.Clients;

namespace Volvox.Helios.Service.Discord.Guild
{
    public class DiscordGuildService : IDiscordGuildService
    {
        private readonly IDiscordAPIClient _client;

        public DiscordGuildService(IDiscordAPIClient client)
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

        /// <inheritdoc />
        public async Task<List<Emoji>> GetEmojis(ulong guildId)
        {
            var emojis = await _client.GetEmojis(guildId);

            return JsonConvert.DeserializeObject<List<Emoji>> ( emojis );
        }

        /// <summary>
        ///     Get the details of the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>Guild populated with details.</returns>
        public async Task<Domain.Discord.Guild> GetDetails(ulong guildId)
        {
            var guild = await _client.GetGuild(guildId);

            return JsonConvert.DeserializeObject<Domain.Discord.Guild>(guild);
        }
    }
}