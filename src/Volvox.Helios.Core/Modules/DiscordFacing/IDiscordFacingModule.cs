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

        /// <summary>
        /// 
        /// </summary>
        Task Initialize();

        /// <summary>
        /// Not to be confused with IModule.Execute. Implement this method with your command execution.
        /// </summary>
        /// <param name="context">Supplied by DiscordFacingManager</param>
        Task ExecuteAsync(DiscordFacingContext context);
    }
}