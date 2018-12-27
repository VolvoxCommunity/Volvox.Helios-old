using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.BypassCheck
{
    public interface IBypassCheck
    {
        bool HasBypassAuthority(ModerationSettings settings, SocketMessage message, WhitelistType type);
    }
}
