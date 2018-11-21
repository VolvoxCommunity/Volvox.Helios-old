using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Web.Models.Moderation;

namespace Volvox.Helios.Web.ViewModels.Moderation
{
    public class PunishmentsViewModel
    {
        public List<PunishmentModel> Punishments { get; set; }

        public List<string> WarningTypes { get; set; }

        public List<string> PunishmentTypes { get; set; }
    }
}
