using System.Threading.Tasks;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <summary>
    /// Unit of the bot.
    /// </summary>
    public abstract class Module : IModule
    {
        protected Module(IDiscordSettings discordSettings)
        {
            DiscordSettings = discordSettings;
        }

        /// <summary>
        /// Start the module.
        /// </summary>
        public virtual Task Start()
        {
            if (IsEnabled)
            {
                Execute();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Execute the module.
        /// </summary>
        public abstract Task Execute();

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