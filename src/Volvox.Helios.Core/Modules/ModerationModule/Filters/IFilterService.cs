using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters
{
    public interface IFilterService
    {
        FilterType GetFilterType();

        bool CheckViolation(ModerationSettings settings, SocketMessage message);

        Task HandleViolation(ModerationSettings settings, SocketMessage message);
    }
}
