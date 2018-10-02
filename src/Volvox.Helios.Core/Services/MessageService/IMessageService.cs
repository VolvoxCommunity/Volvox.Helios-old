using System.Threading.Tasks;
using Discord;

namespace Volvox.Helios.Core.Services.MessageService
{
    public interface IMessageService
    {
        /// <summary>
        ///     Fetch a message by its Id.
        /// </summary>
        /// <param name="messageId">Id of message to fetch.</param>
        /// <param name="channelId">Id of channel message is in.</param>
        /// <returns></returns>
        Task<IUserMessage> GetMessage(ulong messageId, ulong channelId);

        /// <summary>
        ///     Post message to channel.
        /// </summary>
        /// <param name="channelId">Id of channel</param>
        /// <param name="text">Text of message to post.</param>
        /// <param name="embed">Embed of message to post.</param>
        /// <param name="isTTS">Is text to speech.</param>
        /// <param name="options">Extra request options.</param>
        /// <returns>Message after posted to channel.</returns>
        Task<IUserMessage> Post(ulong channelId, string text, Embed embed = null, bool isTTS = false,
            RequestOptions options = null);

        /// <summary>
        ///     Add reaction to message.
        /// </summary>
        /// <param name="message">Message to add reaction to.</param>
        /// <param name="reaction">Reaction to add.</param>
        /// <returns></returns>
        Task AddReaction(IUserMessage message, Emoji reaction);

        /// <summary>
        ///     Modify a message.
        /// </summary>
        /// <param name="message">Message text to change to.</param>
        /// <param name="text">Message text to change to.</param>
        /// <param name="embed">Message embed to change to.</param>
        /// <returns>User message after modification.</returns>
        Task<IUserMessage> Modify(IUserMessage message, string text, Embed embed);

        /// <summary>
        ///     Delete message.
        /// </summary>
        /// <param name="channelId">Channel message is located in.</param>
        /// <param name="messageIds">Array of message ids to delete.</param>
        /// <returns></returns>
        Task Delete(ulong channelId, ulong[] messageIds);
    }
}