using System;
using System.Collections.Generic;
using System.Text;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.Utils.PunishmentFactory
{
    public interface IPunishmentFactory
    {
        IPunishment GetPunishment(PunishType type);
    }
}
