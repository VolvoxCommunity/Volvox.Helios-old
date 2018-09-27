using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module
{
    public class StreamAnnouncerChannelSettings
    {
        [Key]
        public ulong ChannelId { get; set; }

        public ulong GuildId { get; set; }

        public StreamAnnouncerSettings StreamAnnouncerSettings { get; set; }

        public bool RemoveMessage { get; set; }
    }
}
