using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.WarningService
{
    public interface IWarningService
    {
        /// <summary>
        ///     Add warning to database.
        /// </summary>
        /// <param name="user">Discord.net user object of user who violated filter.</param>
        /// <param name="warningType">Type of filter violated</param>
        /// <returns></returns>
        Task<Warning> AddWarning(SocketGuildUser user, FilterType warningType);

        /// <summary>
        ///     Remove warning from db.
        /// </summary>
        /// <param name="warning">Warning to remove.</param>
        /// <returns></returns>
        Task RemoveWarning(Warning warning);

        /// <summary>
        ///     Remove warnings from db.
        /// </summary>
        /// <param name="warnings">Warnings to remove.</param>
        /// <returns></returns>
        Task RemoveWarningBulk(List<Warning> warnings);
    }
}
