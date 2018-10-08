using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter
{
    public class ProfanityFilter : FilterBase
    {
        [Required]
        public List<BannedWord> BannedWords { get; set; }
    }
}
