using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Domain.Module
{
    public class ReactionRolesMessage
    {
        public long Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong? MessageId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public List<ReactionRolesEmoteMapping> RollMappings { get; set; }
    }
}
