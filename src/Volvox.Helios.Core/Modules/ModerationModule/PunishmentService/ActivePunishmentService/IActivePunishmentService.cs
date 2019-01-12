using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.ActivePunishmentService
{
    public interface IActivePunishmentService
    {
        /// <summary>
        ///     Adds collection of punishments to user as active punishments.
        /// </summary>
        /// <param name="punishments">List of punishments.</param>
        /// <param name="userId">Id of user to add punishments to.</param>
        /// <returns></returns>
        Task AddActivePunishments(IEnumerable<Punishment> punishments, int userId);

        /// <summary>
        ///     Removes active punishment from database.
        /// </summary>
        /// <param name="punishment">Punishment to remove</param>
        /// <returns></returns>
        Task RemoveActivePunishmentFromDb(ActivePunishment punishment);

        /// <summary>
        ///     Detects whether user already has a specific punishment already active.
        /// </summary>
        /// <param name="punishment">Punishment Tier</param>
        /// <param name="userData">User entry from db</param>
        /// <returns></returns>
        bool IsPunishmentAlreadyActive(Punishment punishment, UserWarnings userData);
    }
}
