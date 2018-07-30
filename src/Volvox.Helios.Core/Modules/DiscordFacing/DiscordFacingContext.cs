using Discord;
using Discord.WebSocket;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public class DiscordFacingContext
    {
        public SocketUserMessage Message { get; }
        public ISocketMessageChannel Channel { get; }
        public SocketUser User { get; }
        public DiscordSocketClient Client { get; }
        public string GivenPrefix { get; }

        public DiscordFacingContext(SocketUserMessage m, DiscordSocketClient c, string p)
        {
            Message = m;
            Channel = m.Channel;
            User = m.Author;
            Client = c;
            GivenPrefix = p;
        }
    }
}