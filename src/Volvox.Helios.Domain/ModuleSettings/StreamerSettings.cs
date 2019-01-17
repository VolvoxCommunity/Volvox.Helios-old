using System.Collections.Generic;
using Volvox.Helios.Core.Modules.StreamAnnouncer;
using Volvox.Helios.Domain.Module;

namespace Volvox.Helios.Domain.ModuleSettings
{
    public class StreamerSettings : ModuleSettings
    {
        public List<StreamerChannelSettings> ChannelSettings { get; set; }

        public List<StreamAnnouncerMessage> StreamMessages { get; set; }

        public bool StreamerRoleEnabled { get; set; }

        public ulong RoleId { get; set; }

        public List<WhiteListedRole> WhiteListedRoleIds { get; set; }
    }
}