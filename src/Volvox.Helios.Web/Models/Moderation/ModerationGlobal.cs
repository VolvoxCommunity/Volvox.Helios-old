using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Module.ModerationModule;

namespace Volvox.Helios.Web.Models.Moderation
{
    public class ModerationGlobal
    {
        public ulong GuildId { get; set; }

        public List<ulong> WhitelistedChannelIds{ get; set; }

        public List<ulong> WhitelistedRoleIds { get; set; }
    }
}
