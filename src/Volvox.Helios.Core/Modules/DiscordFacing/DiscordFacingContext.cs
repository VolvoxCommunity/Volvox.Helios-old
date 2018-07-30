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
        
        /// <param name="m">Pass the message emitted by your DiscordSocketClient.</param>
        /// <param name="c">Pass the client the emitted the given message.</param>
        /// <param name="p">If the guild the message originated from has configured a custom prefix, pass it here.</param>
        public DiscordFacingContext(SocketUserMessage m, DiscordSocketClient c, string p)
        {
            Message = m;
            Channel = m.Channel;
            User = m.Author;
            Client = c;
        }
    }
}