using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Volvox.Helios.Core.Modules.Common;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class SettingsIndexViewModel
    {
        public ulong GuildId { get; set; }

        public string GuildName { get; set; }

        public IList<IModule> Modules { get; set; }

        public int PollCount { get; set; }

        public int ReactionRolePostCount { get; set; }
    }
}