using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter
{
    public class BannedWord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual ProfanityFilter ProfanityFilter { get; set; }

        [Required]
        public string Word { get; set; }
    }
}
