using System.Threading.Tasks;
using Volvox.Helios.Core.Bot;

namespace Volvox.Helios.Core.Modules.CleanChat
{
    public class CleanChatJob
    {
        private readonly IBot _bot;

        public CleanChatJob(IBot bot)
        {
            _bot = bot;
        }

        public void DeleteMessage(ulong guildId, ulong channelId, ulong messageId)
        {
           _bot.Client.GetGuild(guildId).GetTextChannel(channelId).DeleteMessageAsync(messageId);
        }
    }
}