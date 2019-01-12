using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.BypassCheck
{
    public interface IBypassCheck
    {
        /// <summary>
        ///     Determines whether a user can bypass the module/filter.
        /// </summary>
        /// <param name="message">Message object</param>
        /// <param name="type">Type of filter to check.</param>
        /// <returns></returns>
        Task<bool> HasBypassAuthority(SocketMessage message, FilterType type);
    }
}
