using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService
{
    public interface IPunishmentService
    {
        /// <summary>
        ///     Apply list of punishments.
        /// </summary>
        /// <param name="moderationSettings"> Moderation settings entry for user. </param>
        /// <param name="channelId">Channel Id for posting relevant messages </param>
        /// <param name="punishments">Punishments tp apply</param>
        /// <param name="user">User whom we should apply punishments to.</param>
        /// <param name="userData">User Data entry from our database.</param>
        /// <returns></returns>
        Task ApplyPunishments(List<Punishment> punishments, SocketGuildUser user);

        Task RemovePunishment(ActivePunishment punishment);

        Task RemovePunishmentBulk(List<ActivePunishment> punishments);
    }
}
