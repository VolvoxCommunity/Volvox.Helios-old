using System;
using System.Collections.Generic;
using System.Text;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments
{
    public class PunishmentDataModel
    {
        public PunishType PunishType { get; set; }

        public bool RemovesUserFromGuild { get; set; }
    }
}
