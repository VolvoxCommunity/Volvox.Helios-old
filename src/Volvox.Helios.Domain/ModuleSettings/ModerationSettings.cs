using Volvox.Helios.Domain.Module.ModerationModule;
using System.Collections.Generic;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;

namespace Volvox.Helios.Domain.ModuleSettings
{
    public class ModerationSettings : ModuleSettings
    {
        public LinkFilter LinkFilter { get; set; }

        public ProfanityFilter ProfanityFilter { get; set; }

        public List<UserWarnings> UserWarnings { get; set; }

        public List<Punishment> GlobalPunishments { get; set; }

        public List<Role> GlobalIgnoreRoles { get; set; }
    }
}
