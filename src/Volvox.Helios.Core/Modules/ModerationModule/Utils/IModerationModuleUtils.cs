using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.ModerationModule.Filters;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.Utils
{
    public interface IModerationModuleUtils
    {
        /// <summary>
        ///     Gets moderation module settings from db.
        /// </summary>
        /// <param name="guildId">Guild id to get settings for.</param>
        /// <returns></returns>
        Task<ModerationSettings> GetModerationSettings(ulong guildId);
    }
}
