using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Volvox.Helios.Core.Bot.Connector;
using Volvox.Helios.Core.Modules.Common;

namespace Volvox.Helios.Core.Bot
{
    /// <summary>
    /// Discord bot.
    /// </summary>
    public interface IBot
    {
        /// <summary>
        /// Initialize all of modules available to the bot.
        /// </summary>
        Task InitModules();
        
        /// <summary>
        /// Start the bot.
        /// </summary>
        Task Start();

        /// <summary>
        /// Log an event.
        /// </summary>
        /// <param name="message">Message to log.</param>
        Task Log(LogMessage message);

        /// <summary>
        /// Client for the bot.
        /// </summary>
        DiscordSocketClient Client { get; }

        /// <summary>
        /// Connector that the bot uses to connect to Discord.
        /// </summary>
        IBotConnector Connector { get; }

        /// <summary>
        /// List of modules for the bot.
        /// </summary>
        IList<IModule> Modules { get; }
    }
}