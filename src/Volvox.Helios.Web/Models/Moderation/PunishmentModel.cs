using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Web.Models.Moderation
{
    public class PunishmentModel
    {
        public short WarningThreshold { get; set; }

        public WarningType WarningType { get; set; }

        public PunishType PunishType { get; set; }

        public int PunishDuration { get; set; }

        public ulong RoleId { get; set; }
    }
}
