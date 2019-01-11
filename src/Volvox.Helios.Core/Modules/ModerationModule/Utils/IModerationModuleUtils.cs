using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.ModerationModule.Filters;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Utils
{
    public interface IModerationModuleUtils
    {
        Task<ModerationSettings> GetModerationSettings(ulong guildId);

        IPunishment FetchPunishment(PunishType type);

        IFilterService FetchFilterService(FilterType type);
    }
}
