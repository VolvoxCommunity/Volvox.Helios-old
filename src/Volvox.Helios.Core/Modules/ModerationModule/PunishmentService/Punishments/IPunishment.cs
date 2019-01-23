using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments
{
    public interface IPunishment
    {
        /// <summary>
        ///     Gets punishment meta data.
        /// </summary>
        /// <returns></returns>
        PunishmentMetaData GetPunishmentMetaData();

        /// <summary>
        ///     Apply punishment to user specified.
        /// </summary>
        /// <param name="punishment">Punishment to apply.</param>
        /// <param name="user">User to apply punishment to.</param>
        /// <returns></returns>
        Task<PunishmentResponse> ApplyPunishment(Punishment punishment, SocketGuildUser user);

        /// <summary>
        ///     Remove specified active punishment
        /// </summary>
        /// <param name="punishment">Active punishment to remove.</param>
        /// <returns>Punishment response object.</returns>
        Task<PunishmentResponse> RemovePunishment(ActivePunishment punishment);
    }
}