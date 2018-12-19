using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters.Link
{
    public interface ILinkFilterService
    {
        bool LinkCheck(SocketMessage message, LinkFilter linkFilter);
    }
}
