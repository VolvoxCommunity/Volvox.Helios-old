using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <inheritdoc />
    /// <summary>
    ///     Unit of the bot.
    /// </summary>
    /// <seealso cref="T:Volvox.Helios.Core.Modules.Common.IModule" />
    public abstract class Module : IModule
    {
        protected Module(IDiscordSettings discordSettings, ILogger<IModule> logger)
        {
            DiscordSettings = discordSettings;
            Logger = logger;
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
            if (IsEnabled)
            {
                await Execute(client);
            }
        }

        /// <summary>
        ///     Execute the module.
        /// </summary>
        /// <param name="client">Client for the module to be registered to.</param>
        public virtual Task Execute(DiscordSocketClient client)
        {
            throw new NotImplementedException();
        }

        public IDiscordSettings DiscordSettings { get; }

        public ILogger<IModule> Logger { get; }

        public bool IsEnabled { get; set; } = true;
    }

    public abstract class TriggerableModule : Module
    {
        protected TriggerableModule(IDiscordSettings discordSettings, ITrigger trigger, ILogger<IModule> logger) : base(discordSettings, logger)
        {
            this.Trigger = trigger;
        }

        public ITrigger Trigger { get; }

    }
}