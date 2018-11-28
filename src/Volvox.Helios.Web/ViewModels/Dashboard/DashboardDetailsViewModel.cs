using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Discord;

namespace Volvox.Helios.Web.ViewModels.Dashboard
{
    public class DashboardDetailsViewModel
    {
        public List<UserGuild> UserGuilds { get; set; }

        public Guild Guild { get; set; }
    }
}
