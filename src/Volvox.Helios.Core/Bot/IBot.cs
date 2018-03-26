using System.Threading.Tasks;
using Discord;

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
    }
}