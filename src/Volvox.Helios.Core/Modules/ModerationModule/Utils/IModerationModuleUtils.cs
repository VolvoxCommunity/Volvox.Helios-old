using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Utils
{
    public interface IModerationModuleUtils
    {
        Task<ModerationSettings> GetModerationSettings(ulong guildId);
    }
}
