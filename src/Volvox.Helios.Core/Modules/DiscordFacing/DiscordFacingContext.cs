using Discord.WebSocket;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public class DiscordFacingContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscordFacingContext" /> class.
        /// </summary>
        /// <param name="socketUserMessage">Pass the message emitted by your DiscordSocketClient.</param>
        /// <param name="discordSocketClient">Pass the client the emitted the given message.</param>
        public DiscordFacingContext(SocketUserMessage socketUserMessage, DiscordSocketClient discordSocketClient)
        {
            Message = socketUserMessage;
            Channel = socketUserMessage.Channel;
            User = socketUserMessage.Author;
            Client = discordSocketClient;
        }

        public SocketUserMessage Message { get; }

        public ISocketMessageChannel Channel { get; }

        public SocketUser User { get; }

        public DiscordSocketClient Client { get; }
    }
}