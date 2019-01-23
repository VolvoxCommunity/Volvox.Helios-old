using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.ViolationService
{
    public interface IViolationService
    {
        /// <summary>
        ///     Handles violation by filter type.
        /// </summary>
        /// <param name="message">Discord.net message object which violated filter</param>
        /// <param name="warningType">Type of filter which was violated.</param>
        /// <returns></returns>
        Task HandleViolation(SocketMessage message, FilterType warningType);
    }
}