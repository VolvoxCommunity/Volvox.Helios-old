using System;
using System.Collections.Generic;
using System.Text;
using Volvox.Helios.Domain.Discord;
using Volvox.Helios.Domain.Module.ModerationModule.LinkFilter;
using Volvox.Helios.Domain.Module.ModerationModule.ProfanityFilter;

namespace Volvox.Helios.Domain.ModuleSettings
{
    public class ModerationSettings : ModuleSettings
    {
        public LinkFilter LinkFilter { get; set; }

        public ProfanityFilter ProfanityFilter { get; set; }

        public List<User> User { get; set; }
    }
}
