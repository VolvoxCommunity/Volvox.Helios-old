using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Internal;
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

            var moduleName = GetType().Name;

            // Check if the module exists in the metadata.
            if (config.GetSection($"ModuleMetadata:{moduleName}").Exists())
            {
                Name = config[$"ModuleMetadata:{moduleName}:Name"];
                Version = config[$"ModuleMetadata:{moduleName}:Version"];
                Description = config[$"ModuleMetadata:{moduleName}:Description"];
                Configurable = bool.Parse(config[$"ModuleMetadata:{moduleName}:Configurable"]);
                ReleaseState = Enum.Parse<ReleaseState>(config[$"ModuleMetadata:{moduleName}:ReleaseState"]);
            }

            else
                Logger.LogError($"Module cannot be found in the metadata! Name: {moduleName}");
        }

        /// <summary>
        ///     Initialize the module.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
        public abstract Task Init(DiscordSocketClient client);

        /// <summary>
        ///     Start the module.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
        public virtual async Task Start(DiscordSocketClient client)
        {
            if (IsEnabled) await Execute(client);
        }

        /// <summary>
        ///     Execute the module.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
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

        /// <summary>
        ///     Module name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Module version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        ///     Description of what the module does.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     True if you can manage configuration for the module otherwise false.
        /// </summary>
        public bool Configurable { get; }

        /// <summary>
        ///     Module release state.
        /// </summary>
        public ReleaseState ReleaseState { get; }
    }
}