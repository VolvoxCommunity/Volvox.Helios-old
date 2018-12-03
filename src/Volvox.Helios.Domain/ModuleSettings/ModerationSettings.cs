using Volvox.Helios.Domain.Module.ModerationModule;
using System.Collections.Generic;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volvox.Helios.Domain.ModuleSettings
{
    [Table("mod_ModerationSettings")]
    public class ModerationSettings : ModuleSettings
    {
        public ProfanityFilter ProfanityFilter { get; set; }

        public LinkFilter LinkFilter { get; set; }

        public virtual List<UserWarnings> UserWarnings { get; set; } = new List<UserWarnings>();

        public virtual List<Punishment> Punishments { get; set; } = new List<Punishment>();

        public virtual List<WhitelistedRole> WhitelistedRoles { get; set; } = new List<WhitelistedRole>();

        public virtual List<WhitelistedChannel> WhitelistedChannels { get; set; } = new List<WhitelistedChannel>();
    }
}
