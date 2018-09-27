using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volvox.Helios.Domain.Module;

namespace Volvox.Helios.Domain.ModuleSettings
{
    public class PollSettings
    {
        [Key]
        public ulong GuildId { get; set; }

        public List<Poll> Polls { get; set; }
    }
}
