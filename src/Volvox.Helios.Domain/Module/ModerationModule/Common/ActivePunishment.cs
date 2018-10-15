using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    [Table("mod_ActivePunishments")]
    public class ActivePunishment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual ulong GuildId { get; set; }

        [ForeignKey("GuildId")]
        public virtual ModerationSettings Moderationsettings { get; set; }

        [Required]
        public ulong UserId { get; set; }

        public ulong? RoleId { get; set; }

        [Required]
        public PunishType PunishType { get; set; }

        [Required]
        public DateTimeOffset PunishmentExpires { get; set; }
    }
}
