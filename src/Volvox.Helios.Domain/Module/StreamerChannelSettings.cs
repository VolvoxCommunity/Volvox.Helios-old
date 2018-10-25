using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module
{
    public class StreamerChannelSettings
    {
        [Key]
        public ulong ChannelId { get; set; }

        public ulong GuildId { get; set; }

        public StreamerSettings StreamerSettings { get; set; }

        public bool RemoveMessage { get; set; }
    }
}
