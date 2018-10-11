using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule
{
    [Table("mod_users")]
    public class UserWarnings
    {
        [Key]
        public int Id { get; set; }

        [Required, ForeignKey("GuildId")]
        public ModerationSettings ModerationSettings { get; set; }

        [Required]
        public ulong UserId { get; set; }

        [Required]
        public List<Warning> Warnings { get; set; }
    }
}
