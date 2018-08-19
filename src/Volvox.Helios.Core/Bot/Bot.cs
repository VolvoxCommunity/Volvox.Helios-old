using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot.Connector;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Bot
{
    public class DiscordSocketClientFactory
    {
        public DiscordSocketClient Create()
        {
            // TODO: Convert logging to module
            return new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            });
        }
    }
    /// <summary>
    /// Discord bot.
    /// </summary>
    public class Bot : IBot
    {
        /// <summary>
        /// Discord bot.
        /// </summary>
        /// <param name="modules">List of modules for the bot.</param>
        /// <param name="settings">Settings used to connect to Discord.</param>
        /// <param name="logger">Application logger.</param>
        public Bot(DiscordSocketClient discordSocketClient, IList<IModule> modules, IDiscordSettings settings, ILogger<Bot> logger)
        {
            this.discordSocketClient = discordSocketClient;
            Modules = modules;
            Logger = logger;

            // TODO: Convert logging to module
            discordSocketClient.Log += Log;
            Connector = new BotConnector(settings, discordSocketClient);
        }

        /// <summary>
        /// Start the bot.
        /// </summary>
        public async Task Start()
        {
            await Connector.Connect();

            await Task.Delay(Timeout.Infinite);
        }

        /// <summary>
        /// Stop the bot.
        /// </summary>
        public async Task Stop()
        {
            await Connector.Disconnect();
        }

        public List<SocketGuild> GetGuilds()
        {
            return discordSocketClient.Guilds.ToList();
        }

        /// <summary>
        /// Log an event.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Logger.LogCritical(message.Message);
                    break;
                case LogSeverity.Error:
                    Logger.LogError(message.Message);
                    break;
                case LogSeverity.Warning:
                    Logger.LogWarning(message.Message);
                    break;
                case LogSeverity.Info:
                    Logger.LogInformation(message.Message);
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Logger.LogTrace(message.Message);
                    break;
                default:
                    Logger.LogInformation(message.Message);
                    break;
            }

            return Task.CompletedTask;
        }


        /// <summary>
        /// Client for the bot.
        /// </summary>
        private readonly DiscordSocketClient discordSocketClient;

        /// <summary>
        /// Connector that the bot uses to connect to Discord.
        /// </summary>
        public IBotConnector Connector { get; }

        /// <summary>
        /// List of modules for the bot.
        /// </summary>
        public IList<IModule> Modules { get; }

        /// <summary>
        /// Application logger.
        /// </summary>
        public ILogger<Bot> Logger { get; }
    }
}