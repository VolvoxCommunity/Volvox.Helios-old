using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <summary>
    ///     Unit of the bot.
    /// </summary>
    public interface IModule : IDocumented
    {
        /// <summary>
        ///     Settings for Discord bot.
        /// </summary>
        IDiscordSettings DiscordSettings { get; }

        /// <summary>
        ///     Logger.
        /// </summary>
        ILogger<IModule> Logger { get; }

        /// <summary>
        ///     To execute or not to execute the module. (Default: true)
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        ///     Initialize the module.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
        Task Init(DiscordSocketClient client);

        /// <summary>
        ///     Start the module.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
        Task Start(DiscordSocketClient client);

        /// <summary>
        ///     Execute the module.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
        Task Execute(DiscordSocketClient client);
    }
}