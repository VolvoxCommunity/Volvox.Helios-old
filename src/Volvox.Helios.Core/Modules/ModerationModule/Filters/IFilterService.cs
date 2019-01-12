using System.Threading.Tasks;
using Discord.WebSocket;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters
{
    public interface IFilterService
    {
        /// <summary>
        ///     Checks whether message violates filter.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<bool> CheckViolation(SocketMessage message);

        /// <summary>
        ///     Get meta data for filter.
        /// </summary>
        /// <returns></returns>
        FilterMetaData GetFilterMetaData();

        /// <summary>
        ///     Get period in which warnings expire for this filter.
        /// </summary>
        /// <param name="guildId">Id of guild</param>
        /// <returns>Expire period in minutes</returns>
        Task<int> GetWarningExpirePeriod(ulong guildId);
    }
}
