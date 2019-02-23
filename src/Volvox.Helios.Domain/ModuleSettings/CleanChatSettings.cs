using System.Collections.Generic;
using Volvox.Helios.Domain.Module;

namespace Volvox.Helios.Domain.ModuleSettings
{
    public class CleanChatSettings : ModuleSettings
    {
        public List<CleanChatChannel> Channels { get; set; }

        public int MessageDuration { get; set; }
    }
}