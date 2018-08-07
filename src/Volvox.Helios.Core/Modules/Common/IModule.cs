using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <summary>
    /// Unit of the bot.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        Task Init(DiscordSocketClient client);

        /// <summary>
        /// Start the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        Task Start(DiscordSocketClient client);

        /// <summary>
        /// Execute the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        Task Execute(DiscordSocketClient client);

        /// <summary>
        /// Settings for Discord bot.
        /// </summary>
        IDiscordSettings DiscordSettings { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        ILogger<IModule> Logger { get; }

        /// <summary>
        /// To execute or not to execute the module. (Default: true)
        /// </summary>
        bool IsEnabled { get; set; }
        
        string Name { get; set; }
        string Version { get; set; }
        string Synopsis { get; set; }
    }
}