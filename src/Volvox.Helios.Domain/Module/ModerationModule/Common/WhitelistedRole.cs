using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule
{
    [Table("mod_whitelisted_roles")]
    public class WhitelistedRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual ulong GuildId { get; set; }

        [ForeignKey("GuildId")]
        public virtual ModerationSettings Moderationsettings { get; set; }

        [Required]
        public ulong RoleId { get; set; }

        [Required]
        public WhitelistType WhitelistType { get; set; }
    }
}
