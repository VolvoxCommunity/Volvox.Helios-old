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
        
        /// <param name="message">Pass the message emitted by your DiscordSocketClient.</param>
        /// <param name="client">Pass the client the emitted the given message.</param>
        /// <param name="prefix">If the guild the message originated from has configured a custom prefix, pass it here.</param>
        public DiscordFacingContext(SocketUserMessage message, DiscordSocketClient client, string prefix)
        {
            Message = message;
            Channel = message.Channel;
            User = message.Author;
            Client = client;
            GivenPrefix = prefix;
        }
    }
}