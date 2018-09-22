using Discord.WebSocket;

namespace Volvox.Helios.Core.Modules.DiscordFacing.Framework
{
    public class CommandContext
    {
        public SocketUserMessage Message { get; }
        public ISocketMessageChannel Channel { get; }
        public SocketUser User { get; }
        public DiscordSocketClient Client { get; }
        public string GivenPrefix { get; }

        public CommandContext(SocketUserMessage message, DiscordSocketClient client, string prefix)
        {
            Message = message;
            Channel = message.Channel;
            User = message.Author;
            Client = client;
            GivenPrefix = prefix;
        }
    }
}