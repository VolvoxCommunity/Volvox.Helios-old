using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Volvox.Helios.Service.Clients
{
    public class DiscordAPIClient
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _context;

        public DiscordAPIClient(HttpClient client, IHttpContextAccessor context)
        {
            _client = client;
            _context = context;

            var accessToken = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Get all of the currently logged in users.
        /// </summary>
        /// <returns>List of the logged in users guilds.</returns>
        public async Task<string> GetUserGuilds()
        {
            return await _client.GetStringAsync("users/@me/guilds");
        }

        /// <summary>
        /// Get all of the channels for the specififed guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>List opf channels in the guild.</returns>
        public async Task<string> GetGuildChannels(ulong guildId)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", "NDI2MTg1Njc0OTUwMTgwODY1.DjFLxA.F3n7-lowlbGjpb-fxIkSVJ7XkBs");

            var b = await _client.GetStringAsync($"guilds/{guildId}/channels");

            return b;
        }
    }
}