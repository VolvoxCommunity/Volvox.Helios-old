using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Domain.Module
{
    public class ReactionRoleMessage
    {
        public long Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public string Message { get; set; }

        public List<ReactionRollEmoteMapping> RollMappings { get; set; }
    }
}
