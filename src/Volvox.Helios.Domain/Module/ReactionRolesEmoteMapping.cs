using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Domain.Module
{
    public class ReactionRolesEmoteMapping
    {
        public long Id { get; set; }
        public long ReactionRoleMessageId { get; set; }
        public ulong GuildId { get; set; }
        public ulong EmoteId { get; set; }
        public ulong RoleId { get; set; }
    }
}
