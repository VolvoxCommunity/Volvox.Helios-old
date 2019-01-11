using System.Threading.Tasks;
using Discord.WebSocket;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters
{
    public interface IFilterService
    {
        Task<bool> CheckViolation(SocketMessage message);

        FilterMetaData GetFilterMetaData();

        Task<int> GetWarningExpirePeriod(ulong guildId);
    }
}
