using System;
using System.Threading.Tasks;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public interface IDiscordFacingModule
    {
        /// <summary>
        /// Not to be confused with IModule.Execute. Implement this method with your command execution.
        /// </summary>
        /// <param name="context">Supplied by DiscordFacingManager</param>
        Task ExecuteAsync(DiscordFacingContext context);
    }
}