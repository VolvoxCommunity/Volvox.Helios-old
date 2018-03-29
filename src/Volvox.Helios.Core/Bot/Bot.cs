using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Volvox.Helios.Core.Bot.Connector;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;

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
        public Bot(IList<IModule> modules, IDiscordSettings settings)
        {
            Modules = modules;

            // TODO: Convert logging to module
            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            });

            Client.Log += Log;

            Connector = new BotConnector(settings, Client);
        }

        /// <summary>
        /// Initialize all of modules available to the bot.
        /// </summary>
        public async Task InitModules()
        {
            // TODO: Initialize modules
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
        /// Log an event.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                default:
                    Console.ResetColor();
                    break;
            }

            Console.WriteLine(
                $"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();

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
    }
}