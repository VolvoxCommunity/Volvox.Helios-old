using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <summary>
    /// Unit of the bot.
    /// </summary>
    public abstract class Module : IModule
    {
        /// <summary>
        /// Unit of the bot.
        /// </summary>
        /// <param name="discordSettings">Settings used to connect to Discord.</param>
        protected Module(IDiscordSettings discordSettings)
        {
            DiscordSettings = discordSettings;
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        public abstract Task Init(DiscordSocketClient client);

        /// <summary>
        /// Start the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        public virtual async Task Start(DiscordSocketClient client)
        {
            if (IsEnabled)
            {
                await Execute(client);
            }
        }

        /// <summary>
        /// Execute the module.
        /// </summary>
        /// <param name="client">Client for the module to be registed to.</param>
        public virtual Task Execute(DiscordSocketClient client)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Settings for Discord bot.
        /// </summary>
        public IDiscordSettings DiscordSettings { get; }

        /// <summary>
        /// To execute or not to execute the module. (Default: true)
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}