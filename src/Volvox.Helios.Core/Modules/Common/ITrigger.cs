using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.DiscordFacing;

namespace Volvox.Helios.Core.Modules.Common
{
    /// <summary>
    ///     ITrigger describes a type that decides whether a module should be invoked or not. 
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// Evaluates if the given context is sufficient to invoke the checking module.
        /// </summary>
        /// <param name="context">Given context to check</param>
        /// <returns></returns>
        Task<bool> TryTrigger(DiscordFacingContext context);
    }
}