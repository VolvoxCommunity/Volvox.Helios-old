using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.Moderation
{
    public class LinkFilterViewModel
    {
        public bool Enabled { get; set; }

        [DisplayName("Whitelisted Channels")]
        public MultiSelectList WhitelistedChannels { get; set; }

        [DisplayName("Whitelisted Roles")]
        public MultiSelectList WhitelistedRoles { get; set; }

        public List<string> WhitelistedLinks { get; set; }
    }
}
