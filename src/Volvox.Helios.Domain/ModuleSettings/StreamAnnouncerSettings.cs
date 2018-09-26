using System.Collections.Generic;
using Volvox.Helios.Core.Modules.StreamAnnouncer;
using Volvox.Helios.Domain.Module;

namespace Volvox.Helios.Domain.ModuleSettings
{
    public class StreamAnnouncerSettings : ModuleSettings
    {
        public List<StreamAnnouncerChannelSettings> ChannelSettings { get; set; }

        public List<StreamAnnouncerMessages> StreamMessages { get; set; }
    }
}