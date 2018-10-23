using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Volvox.Helios.Core.Bot;

namespace Volvox.Helios.Core.Services.MessageService
{
    public class MessageService : IMessageService
    {
        #region static props

        // Unicode representations of discord's 0-10 emoticons.
        public static readonly string[] DiscordNumberEmotes =
        {
            "0\u20e3", "1\u20e3", "2\u20e3", "3\u20e3", "4\u20e3", "5\u20e3", "6\u20e3", "7\u20e3", "8\u20e3",
            "9\u20e3", "🔟"
        };

        #endregion

        private readonly DiscordSocketClient _client;

        public MessageService(DiscordSocketClient client)
        {
            _client = client;
        }

        ///<inheritdoc />
        public async Task<IUserMessage> GetMessage(ulong channelId, ulong messageId)
        {
            var channel = GetChannel(channelId);

            if (channel == null) return null;

            var message = await channel.GetMessageAsync(messageId) as IUserMessage;

            return message;
        }

        ///<inheritdoc />
        public Task<IUserMessage> Post(ulong channelId, string text, Embed embed = null, bool isTTS = false,
            RequestOptions options = null)
        {
            var channel = GetChannel(channelId);

            return channel?.SendMessageAsync(text, isTTS, embed, options);
        }

        ///<inheritdoc />
        public Task AddReaction(IUserMessage message, Emoji reaction)
        {
            return message.AddReactionAsync(reaction);
        }

        ///<inheritdoc />
        public async Task<IUserMessage> Modify(IUserMessage message, string text = "", Embed embed = null)
        {
            await message.ModifyAsync(m =>
            {
                m.Content = text;
                m.Embed = embed;
            });

            return message;
        }

        /// <summary>
        ///     Get a channel by id.
        /// </summary>
        /// <param name="channelId">Id of the channel to get.</param>
        /// <returns>IMessageChannel.</returns>
        private IMessageChannel GetChannel(ulong channelId)
        {
            return _client.GetChannel(channelId) as IMessageChannel;
        }
    }
}