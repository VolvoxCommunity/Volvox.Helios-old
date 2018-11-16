using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.Moderation
{
    public class ProfanityFilterViewModel
    {
        public bool Enabled { get; set; }

        [DisplayName("Whitelisted Channels")]
        public MultiSelectList WhitelistedChannels { get; set; }

        public List<ulong> SelectedChannels { get; set; }

        [DisplayName("Whitelisted Roles")]
        public MultiSelectList WhitelistedRoles { get; set; }

        public List<ulong> SelectedRoles { get; set; }

        [DisplayName("Use default banned words")]
        public bool UseDefaultBannedWords { get; set; }

        [DisplayName("Banned Words")]
        public MultiSelectList BannedWords { get; set; }

        [DisplayName("Add New")]
        public HashSet<string> SelectedWords { get; set; }

        [DisplayName("Warning expire period in minutes (0 = warnings don't expire).")]
        public int WarningExpirePeriod { get; set; }
    }
}
