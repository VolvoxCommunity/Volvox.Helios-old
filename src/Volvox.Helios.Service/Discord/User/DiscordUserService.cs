using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Volvox.Helios.Service.Clients;

namespace Volvox.Helios.Service.Discord.User
{
    public class DiscordUserService : IDiscordUserService
    {
        private readonly DiscordAPIClient _client;

        public DiscordUserService(DiscordAPIClient client)
        {
            _client = client;
        }

        /// <summary>
        ///     Get all of the logged in users guilds.
        /// </summary>
        /// <returns>List of all of the logged in users guilds.</returns>
        public async Task<List<Domain.Discord.Guild>> GetUserGuilds()
        {
            var guilds = await _client.GetUserGuilds();

            return JsonConvert.DeserializeObject<List<Domain.Discord.Guild>>(guilds);
        }
    }
}