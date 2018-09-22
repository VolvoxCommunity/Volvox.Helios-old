using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot.Connector;
using Volvox.Helios.Core.Modules.Common;

namespace Volvox.Helios.Core.Bot
{
    /// <summary>
    ///     Discord bot.
    /// </summary>
    public interface IBot
    {
        /// <summary>
        ///     Client for the bot.
        /// </summary>
        DiscordSocketClient Client { get; }

        /// <summary>
        ///     Connector that the bot uses to connect to Discord.
        /// </summary>
        IBotConnector Connector { get; }

        /// <summary>
        ///     List of modules for the bot.
        /// </summary>
        IList<IModule> Modules { get; }

        /// <summary>
        ///     Application logger.
        /// </summary>
        ILogger<Bot> Logger { get; }

        /// <summary>
        ///     Initialize all of modules available to the bot.
        /// </summary>
        Task InitModules();

        /// <summary>
        ///     Start the bot.
        /// </summary>
        Task Start();

        /// <summary>
        ///     Stop the bot.
        /// </summary>
        Task Stop();

        /// <summary>
        ///     Get a list of the guilds the bot is in.
        /// </summary>
        /// <returns>List of the guilds the bot is in.</returns>
        IReadOnlyCollection<SocketGuild> GetGuilds();

        /// <summary>
        ///     Returns true if the specified guild is in the bot and false otherwise.
        /// </summary>
        /// <returns>Returns true if the specified guild is in the bot and false otherwise.</returns>
        bool IsBotInGuild(ulong guildId);

        /// <summary>
        ///     Get the bots role in the hierarchy of the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild to get the hierarchy from.</param>
        /// <returns>Bots role position.</returns>
        int GetBotRoleHierarchy(ulong guildId);

        /// <summary>
        ///     Log an event.
        /// </summary>
        /// <param name="message">Message to log.</param>
        Task Log(LogMessage message);
    }
}