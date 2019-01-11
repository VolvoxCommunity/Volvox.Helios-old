using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters
{
    public interface IFilterService
    {
        Task<bool> CheckViolation(SocketMessage message);

        Task HandleViolation(SocketMessage message);

        FilterMetaData GetFilterMetaData();

        Task<int> GetWarningExpirePeriod(ulong guildId);
    }
}
