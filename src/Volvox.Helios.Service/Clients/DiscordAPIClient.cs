using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentCache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Volvox.Helios.Service.Clients
{
    public class DiscordAPIClient
    {
        private readonly ICache _cache;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;

        public DiscordAPIClient(HttpClient client, IHttpContextAccessor context, IConfiguration configuration,
            ICache cache)
        {
            _client = client;
            _context = context;
            _configuration = configuration;
            _cache = cache;

            // Set access token.
            var accessToken = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        ///     Get all of the currently logged in users guilds.
        /// </summary>
        /// <returns>JSON array of the logged in users guilds.</returns>
        public async Task<string> GetUserGuilds()
        {
            // Cache the users guilds.
            var cachedUserGuilds = await _cache.WithKey($"UserGuilds:{GetUserId()}")
                .RetrieveUsingAsync(async () => await _client.GetStringAsync("users/@me/guilds"))
                .InvalidateIf(cachedValue => cachedValue.Value != null)
                .ExpireAfter(TimeSpan.FromSeconds(30))
                .GetValueAsync();

            return cachedUserGuilds;
        }

        /// <summary>
        ///     Get all of the channels in the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>JSON array of channels in the guild.</returns>
        public async Task<string> GetGuildChannels(ulong guildId)
        {
            SetBotToken();

            return await _client.GetStringAsync($"guilds/{guildId}/channels");
        }

        /// <summary>
        ///     Get all of the roles in the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns>JSON array of roles in the guild.</returns>
        public async Task<string> GetGuildRoles(ulong guildId)
        {
            SetBotToken();

            return await _client.GetStringAsync($"guilds/{guildId}/roles");
        }

        /// <summary>
        ///     Add the bot token to the authentication header.
        /// </summary>
        private void SetBotToken()
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bot", _configuration["Discord:Token"]);
        }

        /// <summary>
        ///     Get the id of the currently logged in user.
        /// </summary>
        /// <returns>Id of the currently logged in user.</returns>
        private ulong GetUserId()
        {
            var value = _context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier"))?.Value;

            return value != null ? ulong.Parse(value) : 0;
        }
    }
}