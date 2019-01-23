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
        /// <param name="punishments">Punishments to apply</param>
        /// <param name="user">User whom we should apply punishments to.</param>
        /// <returns></returns>
        Task ApplyPunishments(List<Punishment> punishments, SocketGuildUser user);

        /// <summary>
        ///     Removes punishment from user.
        /// </summary>
        /// <param name="punishment">Punishment to remove.</param>
        /// <returns></returns>
        Task RemovePunishment(ActivePunishment punishment);

        /// <summary>
        ///     Removes group of punishments from user.
        /// </summary>
        /// <param name="punishments">Punishments to remove.</param>
        /// <returns></returns>
        Task RemovePunishmentBulk(List<ActivePunishment> punishments);
    }
}