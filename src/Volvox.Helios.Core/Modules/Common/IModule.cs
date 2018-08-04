using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <summary>
    ///     Unit of the bot.
    /// </summary>
    public interface IModule
    {
        IDiscordSettings DiscordSettings { get; }

        ILogger<IModule> Logger { get; }

        bool IsEnabled { get; set; }

        /// <summary>
        ///     Initializes the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        Task Init(DiscordSocketClient client);

        /// <summary>
        ///     Starts the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        Task Start(DiscordSocketClient client);

        /// <summary>
        ///     Executes the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        Task Execute(DiscordSocketClient client);
    }
}