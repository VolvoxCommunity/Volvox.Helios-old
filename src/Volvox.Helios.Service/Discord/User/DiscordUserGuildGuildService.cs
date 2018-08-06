using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Volvox.Helios.Domain.Discord;
using Volvox.Helios.Domain.JsonConverters;
using Volvox.Helios.Service.Clients;

namespace Volvox.Helios.Service.Discord.User
{
    public class DiscordUserGuildGuildService : IDiscordUserGuildService
    {
        private readonly DiscordAPIClient _client;

        public DiscordUserGuildGuildService(DiscordAPIClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Get all of the logged in users guilds.
        /// </summary>
        /// <returns>List of all of the logged in users guilds.</returns>
        public async Task<List<UserGuild>> GetUserGuilds()
        {
            var guildsResponse = await _client.GetUserGuilds();

            return JsonConvert.DeserializeObject<List<UserGuild>>(guildsResponse, new UserGuildJsonConverter());
        }
    }
}