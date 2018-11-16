using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Volvox.Helios.Web.Models.Moderation
{
    public class WhitelistedLinkModel
    {
        public string Link { get; set; }

        public ulong ChannelId { get; set; }
    }
}
