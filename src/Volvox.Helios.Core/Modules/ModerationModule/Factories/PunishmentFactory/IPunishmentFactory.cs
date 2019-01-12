using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.Factories.PunishmentFactory
{
    public interface IPunishmentFactory
    {
        IPunishment GetPunishment(PunishType type);
    }
}
