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

        public List<UserWarnings> UserWarnings { get; set; }

        public List<Punishment> Punishments { get; set; }

        public List<WhitelistedRole> WhitelistedRoles { get; set; }

        public List<WhitelistedChannel> WhitelistedChannels { get; set; }
    }
}
