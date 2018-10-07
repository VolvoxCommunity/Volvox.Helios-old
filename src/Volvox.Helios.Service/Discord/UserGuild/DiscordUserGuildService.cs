using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Volvox.Helios.Domain.JsonConverters;
using Volvox.Helios.Service.Clients;

namespace Volvox.Helios.Service.Discord.UserGuild
{
    public class DiscordUserGuildService : IDiscordUserGuildService
    {
        private readonly DiscordAPIClient _client;

        public DiscordUserGuildService(DiscordAPIClient client)
        {
            _client = client;
        }

        /// <summary>
        ///     Get all of the logged in users guilds.
        /// </summary>
        /// <returns>List of all of the logged in users guilds.</returns>
        public async Task<List<Domain.Discord.UserGuild>> GetUserGuilds()
        {
            var guildsResponse = await _client.GetUserGuilds();

            return JsonConvert.DeserializeObject<List<Domain.Discord.UserGuild>>(guildsResponse,
                new UserGuildJsonConverter());
        }
    }
}