using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.DiscordFacing;

namespace Volvox.Helios.Core.Modules.Common
{
    public interface ITriggerable
    {
        /// <summary>
        /// Evaluates if the given context is sufficient to invoke the checking module.
        /// </summary>
        /// <param name="context">Given context to check</param>
        /// <returns></returns>
        Task<bool> TryTrigger(DiscordFacingContext context);
    }
}