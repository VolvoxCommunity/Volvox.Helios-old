using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
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
        /// Get all of the channels for the specififed guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>List opf channels in the guild.</returns>
        public async Task<List<Channel>> GetChannels(ulong guildId)
        {
            var channels = await _client.GetGuildChannels(guildId);

            return JsonConvert.DeserializeObject<List<Channel>>(channels);
        }
    }
}
