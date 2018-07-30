using Discord;
using Discord.WebSocket;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public class DiscordFacingContext
    {
        /// <summary>
        /// Emitted by DiscordFacingManager's bound client
        /// </summary>
        public SocketUserMessage Message { get; }
        
        /// <summary>
        /// Taken from Message
        /// </summary>
        public ISocketMessageChannel Channel { get; }
        
        /// <summary>
        /// Taken from Message
        /// </summary>
        public SocketUser User { get; }
        
        /// <summary>
        /// Fed by DiscordFacingManager
        /// </summary>
        public DiscordSocketClient Client { get; }
        
        /// <summary>
        /// The prefix the invoking guiild has set for the bot. Useful once Bapes finishes the settings framework.
        /// </summary>
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