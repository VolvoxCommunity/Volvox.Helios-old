using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    public abstract class FilterBase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [Required]
        public virtual ModerationSettings ModerationSettings { get; set; }

        // Length of time a warning has until it expires.
        [Required]
        public short WarningExpirePeriod { get; set; }

        [Required]
        public List<Punishment> Punishments { get; set; }
        
        // Roles that ignore the filter.
        [Required]
        public List<Role> IgnoreRoles { get; set; }
    }
}
