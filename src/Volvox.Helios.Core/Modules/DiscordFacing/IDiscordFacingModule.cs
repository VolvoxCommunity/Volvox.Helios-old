using System;
using System.Threading.Tasks;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public interface IDiscordFacingModule
    {
        /// <summary>
        /// DiscordFacingManager will loop through its list of modules, checking if the given message matches this trigger
        /// If there's a match, DiscordFacingManager will call ExecuteAsync on that module.
        /// </summary>
        string Trigger { get; }

        Task Initialize();

        Task ExecuteAsync(DiscordFacingContext context);
    }
}