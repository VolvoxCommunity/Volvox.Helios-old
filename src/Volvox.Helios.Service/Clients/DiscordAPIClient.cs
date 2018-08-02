using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Volvox.Helios.Service.Clients
{
    public class DiscordAPIClient
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _context;
        private readonly IConfiguration _configuration;

        public DiscordAPIClient(HttpClient client, IHttpContextAccessor context, IConfiguration configuration)
        {
            _client = client;
            _context = context;
            _configuration = configuration;

            // Set access token.
            var accessToken = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Get all of the currently logged in users.
        /// </summary>
        /// <returns>List of the logged in users guilds.</returns>
        public async Task<string> GetUserGuilds()
        {
            // TOOD: Add caching
            return await _client.GetStringAsync("users/@me/guilds");
        }

        /// <summary>
        /// Get all of the channels for the specififed guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>List opf channels in the guild.</returns>
        public async Task<string> GetGuildChannels(ulong guildId)
        {
            // Set bot token.
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", _configuration["Discord:Token"]);

            return await _client.GetStringAsync($"guilds/{guildId}/channels");
        }
    }
}