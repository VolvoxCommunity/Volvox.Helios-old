using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.ViolationService
{
    public interface IViolationService
    {
        Task HandleViolation(SocketMessage message, FilterType warningType);
    }
}
