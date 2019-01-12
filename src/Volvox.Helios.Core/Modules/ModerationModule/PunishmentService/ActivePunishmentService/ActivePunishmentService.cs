using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Core.Jobs;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.ActivePunishmentService
{
    public class ActivePunishmentService : IActivePunishmentService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ActivePunishmentService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <inheritdoc />
        public async Task AddActivePunishments(IEnumerable<Punishment> punishments, UserWarnings userData)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var activePunishmentsService = scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

                var userWarningsService = scope.ServiceProvider.GetRequiredService<IEntityService<UserWarnings>>();

                var activePunishments = new List<ActivePunishment>();

                foreach (var punishment in punishments)
                {
                    // TODO : Don't manually check for kick here, do it better. perhaps have a field on punishment type checking if the punishment has a duration.
                    // No need to add kick punishment to DB as it's not a punishment with a duration.
                    if (punishment.PunishType == PunishType.Kick)
                        continue;

                    // If PunishDuration is 0, user never wants punishment to expire.
                    var expireDate = punishment.PunishDuration == 0
                        ? DateTimeOffset.MaxValue
                        : DateTimeOffset.Now.AddMinutes(punishment.PunishDuration);

                    // Re-fetching prevents self referencing loop AND ensures the entity is tracked, therefore stopping ef core from trying to re-add it.
                    var userDbEntry = await userWarningsService.Find(userData.Id);

                    activePunishments.Add(new ActivePunishment
                    {
                        PunishmentExpires = expireDate,
                        PunishType = punishment.PunishType,
                        PunishmentId = punishment.Id,
                        RoleId = punishment.RoleId,
                        User = userDbEntry
                    });
                }

                var removePunishmentService = scope.ServiceProvider.GetRequiredService<RemovePunishmentJob>();

                await activePunishmentsService.CreateBulk(activePunishments.Where(p => p.PunishmentExpires > DateTimeOffset.Now));

                // Schedule removal of active punishments
                foreach (var p in activePunishments)
                {
                    if (p.PunishmentExpires > DateTimeOffset.Now && p.PunishmentExpires != DateTimeOffset.MaxValue)
                    {
                        removePunishmentService.ScheduleActivePunishmentRemoval(p);
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task RemoveActivePunishmentFromDb(ActivePunishment punishment)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var activePunishmentService = scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

                await activePunishmentService.Remove(punishment);
            }
        }

        /// <inheritdoc />
        public bool IsPunishmentAlreadyActive(Punishment punishment, UserWarnings userData)
        {
            var currentlyActivePunishments = userData.ActivePunishments;

            // bool indicating whether user already has punishment.
            return currentlyActivePunishments.Any(x => x.PunishmentId == punishment.Id);
        }
    }
}
