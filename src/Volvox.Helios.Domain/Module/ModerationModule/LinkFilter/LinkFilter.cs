using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Domain.Module.ModerationModule.LinkFilter
{
    [Table("mod_link_filters")]
    public class LinkFilter : FilterBase
    {
        [Required] public virtual List<WhitelistedLink> WhitelistedLinks { get; set; } = new List<WhitelistedLink>();
    }
}