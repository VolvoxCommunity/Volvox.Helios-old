using System.Collections.Generic;
using System.Collections.ObjectModel;
using Discord.WebSocket;

namespace Volvox.Helios.Domain.Discord
{
    public class GuildDetails
    {
        public bool IsBotInGuild { get; set; }

        public int MemberCount { get; set; }

        public IReadOnlyCollection<SocketRole> Roles { get; set; } = new Collection<SocketRole>();

        public IReadOnlyCollection<SocketChannel> Channels { get; set; } = new Collection<SocketChannel>();
    }
}