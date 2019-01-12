using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.Factories.PunishmentFactory
{
    public interface IPunishmentFactory
    {
        /// <summary>
        ///     Gets punishment method by type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IPunishment GetPunishment(PunishType type);
    }
}
