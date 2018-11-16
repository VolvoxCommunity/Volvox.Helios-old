using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    public abstract class FilterBase
    {
        [Key, ForeignKey(nameof(ModerationSettings))]
        public ulong GuildId { get; set; }

        public virtual ModerationSettings ModerationSettings { get; set; }

        [Required]
        public bool Enabled { get; set; }

        // Length of time a warning has until it expires.
        [Required]
        public int WarningExpirePeriod { get; set; }
    }
}
