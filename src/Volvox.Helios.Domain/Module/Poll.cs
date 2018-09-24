using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module
{
    public class Poll
    {
        [Key]
        public int Id { get; set; }

        public ulong MessageId { get; set; }

        public ulong ChannelId { get; set; }

        public ulong GuildId { get; set; }

        [MaxLength(150)]
        public string PollTitle { get; set; }

        public virtual PollSettings PollSettings { get; set; }
    }
}
