using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Bot.Connector
{
    /// <summary>
    ///     Discord bot connector.
    /// </summary>
    public class BotConnector : IBotConnector
    {
        private readonly DiscordSocketClient _client;
        private readonly IDiscordSettings _discordSettings;

        /// <summary>
        ///     Discord bot connector.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="client">Client used to connect to Discord.</param>
        public BotConnector(IDiscordSettings discordSettings, DiscordSocketClient client)
        {
            _discordSettings = discordSettings;
            _client = client;
        }

        /// <summary>
        ///     Connect the bot to Discord.
        /// </summary>
        public async Task Connect()
        {
            await _client.LoginAsync(TokenType.Bot, _discordSettings.Token);

            await _client.StartAsync();
        }

        /// <summary>
        ///     Discconect the bot from Discord.
        /// </summary>
        public async Task Disconnect()
        {
            await _client.LogoutAsync();

            await _client.StopAsync();
        }
    }
}