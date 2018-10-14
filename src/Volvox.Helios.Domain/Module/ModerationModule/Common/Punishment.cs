using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    [Table("mod_punishments")]
    public class Punishment
    {
        [Key]
        public int Id { get; set; }

        [Required, ForeignKey("GuildId")]
        public virtual ModerationSettings Moderationsettings { get; set; }

        // Number of warnings until punishment is given
        [Required]
        public short WarningThreshold { get; set; }

        [Required]
        public PunishType PunishType { get; set; }

        [Required]
        public WarningType WarningType { get; set; }

        // Duration of punishment in minutes. null == no duration / forever.
        public short? PunishDuration { get; set; }

        // Id of role to apply when punishment threshold met.
        public ulong? RoleId { get; set; }
    }
}
