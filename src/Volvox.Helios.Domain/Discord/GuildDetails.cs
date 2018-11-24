using System.Collections.Generic;

namespace Volvox.Helios.Domain.Discord
{
    public class GuildDetails
    {
        public bool IsBotInGuild { get; set; }

        public int MemberCount { get; set; }

        public List<Role> Roles { get; set; }

        public List<Channel> Channels { get; set; }
    }
}