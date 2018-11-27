using System.Collections.Generic;
using Discord.WebSocket;

namespace Volvox.Helios.Domain.Discord
{
    public class GuildDetails
    {
        public bool IsBotInGuild { get; set; }

        public int MemberCount { get; set; }

        public IReadOnlyCollection<SocketRole> Roles { get; set; }

        public IReadOnlyCollection<SocketChannel> Channels { get; set; }
    }
}