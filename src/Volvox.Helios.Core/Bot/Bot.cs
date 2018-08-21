using System;
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
using Volvox.Helios.Domain.Discord;

namespace Volvox.Helios.Core.Bot
{
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
        public Bot(IList<IModule> modules, IDiscordSettings settings, ILogger<Bot> logger)
        {
            Modules = modules;
            Logger = logger;

            // TODO: Convert logging to module
            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            });

            Client.Log += Log;

            // Log when the bot is disconnected.
            Client.Disconnected += exception =>
            {
                Logger.LogCritical("Bot has been disconnected!");
                
                return Task.CompletedTask;
            };
            
            Connector = new BotConnector(settings, Client);
        }

        /// <summary>
        /// Initialize all of modules available to the bot.
        /// </summary>
        public async Task InitModules()
        {
            foreach (var module in Modules)
            {
                Logger.LogInformation($"Initializing {module.GetType().Name}");
                await module.Init(Client);
            }
        }

        /// <summary>
        /// Start the bot.
        /// </summary>
        public async Task Start()
        {
            await InitModules();

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

        /// <summary>
        /// Get a list of the guilds the bot is in.
        /// </summary>
        /// <returns>List of the guilds the bot is in.</returns>
        public IReadOnlyCollection<SocketGuild> GetGuilds()
        {
            return Client.Guilds;
        }

        /// <summary>
        /// Returns true if the specified guild is in the bot and false otherwise.
        /// </summary>
        /// <returns>Returns true if the specified guild is in the bot and false otherwise.</returns>
        public bool IsGuildInBot(Guild guild)
        {
            return GetGuilds().Any(g => g.Id == guild.Id);
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
        public DiscordSocketClient Client { get; }

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