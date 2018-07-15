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

        public DiscordAPIClient(HttpClient client, IHttpContextAccessor context)
        {
            _client = client;
            
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
    }
}