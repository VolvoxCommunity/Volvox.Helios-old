using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters
{
    public interface IFilterService<T>
    {
        bool CheckViolation(ModerationSettings settings, SocketMessage message);

        Task HandleViolation(ModerationSettings settings, SocketMessage message);
    }
}
