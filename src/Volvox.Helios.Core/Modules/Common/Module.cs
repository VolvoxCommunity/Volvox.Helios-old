using System.Threading.Tasks;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Common
{
    public abstract class Module : IModule
    {
        public IDiscordSettings DiscordSettings { get; }

        protected Module(IDiscordSettings discordSettings)
        {
            DiscordSettings = discordSettings;
        }

        public abstract Task Execute();
    }
}