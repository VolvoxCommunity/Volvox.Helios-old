using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Volvox.Helios.Web.Models.Moderation
{
    public class ModerationProfanity
    {
        public ulong GuildId { get; set; }

        public bool Enabled { get; set; }

        public List<ulong> BannedWords { get; set; }

        public List<ulong> WhitelistedChannels { get; set; }

        public List<ulong> WhitelistedRoles { get; set; }

        public short WarningExpirePeriod { get; set; }
    }
}
