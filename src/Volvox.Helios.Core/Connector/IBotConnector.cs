using System.Threading.Tasks;

namespace Volvox.Helios.Core.Connector
{
    /// <summary>
    /// Discord bot connector.
    /// </summary>
    public interface IBotConnector
    {
        /// <summary>
        /// Connect the bot to Discord.
        /// </summary>
        Task Connect();

        /// <summary>
        /// Connect the bot from Discord.
        /// </summary>
        Task Disconnect();
    }
}