using Discord.WebSocket;

namespace Volvox.Helios.Core.Modules.ModerationModule.Filters
{
    public interface IFilterService<T>
    {
        bool CheckViolation(T filter, SocketMessage message);

        void HandleViolation(T filter, SocketMessage message);
    }
}
