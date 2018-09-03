using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <summary>
    ///     Unit of the bot.
    /// </summary>
    public abstract class Module : IModule
    {
        /// <summary>
        ///     Unit of the bot.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="config">Application configuration.</param>
        protected Module(IDiscordSettings discordSettings, ILogger<IModule> logger, IConfiguration config)
        {
            DiscordSettings = discordSettings;
            Logger = logger;

            var moduleQuery = GetType().Name;
            Name = config[$"Metadata:{moduleQuery}:Name"];
            Version = config[$"Metadata:{moduleQuery}:Version"];
            Description = config[$"Metadata:{moduleQuery}:Description"];
            ReleaseState = Enum.Parse<ReleaseState>(config[$"Metadata:{moduleQuery}:ReleaseState"]);
            Configurable = bool.Parse(config[$"Metadata:{moduleQuery}:Configurable"]);
        }

        /// <summary>
        ///     Initialize the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        public abstract Task Init(DiscordSocketClient client);

        /// <summary>
        ///     Start the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        public virtual async Task Start(DiscordSocketClient client)
        {
            if (IsEnabled) await Execute(client);
        }

        /// <summary>
        ///     Execute the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        public virtual Task Execute(DiscordSocketClient client)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Settings for Discord bot.
        /// </summary>
        public IDiscordSettings DiscordSettings { get; }

        /// <summary>
        ///     Logger.
        /// </summary>
        public ILogger<IModule> Logger { get; }

        /// <summary>
        ///     To execute or not to execute the module. (Default: true)
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public bool Configurable { get; }
        public ReleaseState ReleaseState { get; set; }
    }
}