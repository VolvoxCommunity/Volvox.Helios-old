using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using Volvox.Helios.Domain.JsonConverters;
using Volvox.Helios.Service.Clients;

namespace Volvox.Helios.Service.Discord.User
{
    public class DiscordUserService : IDiscordUserService
    {
        private readonly IDiscordAPIClient _client;

        private readonly DiscordSocketClient _socketClient;

        public DiscordUserService(IDiscordAPIClient client, DiscordSocketClient socketClient)
        {
            _client = client;

            _socketClient = socketClient;
        }

        /// <summary>
        ///     Get the specified user.
        /// </summary>
        /// <param name="userId">Id of the user to get.</param>
        /// <returns>Specified user.</returns>
        public async Task<Domain.Discord.DiscordUser> GetUser(ulong userId)
        {
            var userResponse = await _client.GetUser(userId);

            return JsonConvert.DeserializeObject<Domain.Discord.DiscordUser>(userResponse, new UserGuildJsonConverter());
        }

        /// <inheritdoc />
        public List<SocketGuildUser> GetUsers(ulong guildId)
        {
            return new List<SocketGuildUser>(_socketClient.GetGuild(guildId).Users);
        }
    }
}