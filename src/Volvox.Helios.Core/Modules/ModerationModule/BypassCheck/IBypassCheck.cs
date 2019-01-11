using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.BypassCheck
{
    public interface IBypassCheck
    {
        Task<bool> HasBypassAuthority(SocketMessage message, FilterType type);
    }
}
