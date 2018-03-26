using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Connector
{
    /// <summary>
    /// Discord bot connector.
    /// </summary>
    public class BotConnector : IBotConnector
    {
        private readonly IDiscordSettings _discordSettings;
        private readonly DiscordSocketClient _client;

        public BotConnector(IDiscordSettings discordSettings, DiscordSocketClient client)
        {
            _discordSettings = discordSettings;
            _client = client;
        }

        /// <summary>
        /// Connect the bot to Discord.
        /// </summary>
        public async Task Connect()
        {
            await _client.LoginAsync(TokenType.Bot, _discordSettings.Token);
        }

        /// <summary>
        /// Connect the bot from Discord.
        /// </summary>
        public async Task Disconnect()
        {
            await _client.LogoutAsync();
        }
    }
}