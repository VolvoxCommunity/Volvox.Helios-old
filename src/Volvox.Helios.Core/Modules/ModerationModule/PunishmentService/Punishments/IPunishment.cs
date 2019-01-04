using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments
{
    public interface IPunishment
    {
        PunishmentDataModel GetPunishmentTypeDetails();

        Task<PunishmentResponse> ApplyPunishment(Punishment punishment, SocketGuildUser user);

        Task<PunishmentResponse> RemovePunishment(ActivePunishment punishment);
    }
}
