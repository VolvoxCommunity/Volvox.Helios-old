using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    public class Punishment
    {
        [Key]
        public int Id { get; set; }

        // Number of warnings until punishment is given
        [Required]
        public short WarningThreshold { get; set; }

        [Required]
        public PunishType PunishType { get; set; }

        // Duration of punishment in minutes. 0 == no duration / forever.
        [Required]
        public short PunishDuration { get; set; }
    }
}
