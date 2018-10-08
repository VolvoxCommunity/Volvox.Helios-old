using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Domain.Module.ModerationModule.LinkFilter
{
    public class LinkFilter : FilterBase
    {
        [Required]
        public List<WhitelistedLink> WhitelistedLinks { get; set; }
    }
}
