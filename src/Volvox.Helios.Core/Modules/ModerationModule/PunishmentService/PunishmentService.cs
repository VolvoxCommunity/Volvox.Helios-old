using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Core.Jobs;
using Volvox.Helios.Core.Modules.ModerationModule.Factories.PunishmentFactory;
using Volvox.Helios.Core.Modules.ModerationModule.UserWarningsService;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService
{
    public class PunishmentService : IPunishmentService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IUserWarningsService _userWarningService;

        private readonly IPunishmentFactory _punishmentFactory;

        public PunishmentService(IServiceScopeFactory scopeFactory, IUserWarningsService userWarningService,
            IPunishmentFactory punishmentFactory)
        {
            _scopeFactory = scopeFactory;

            _userWarningService = userWarningService;

            _punishmentFactory = punishmentFactory;
        }

        /// <inheritdoc />
        public async Task ApplyPunishments(List<Punishment> punishments, SocketGuildUser user)
        {
            var userData = await _userWarningService.GetUser(user.Id, user.Guild.Id, u => u.ActivePunishments);

            // List of punishments to add to database as active punishments.
            var activePunishments = new List<Punishment>();

            foreach (var punishment in punishments)
            {
                // Check to make sure user doesn't already have this punishment. This could cause issues if the same punishment is applied twice.
                if (IsPunishmentAlreadyActive(punishment, userData))
                    continue;

                var punishmentMethodService = _punishmentFactory.GetPunishment(punishment.PunishType);

                var punishmentResponse = await punishmentMethodService.ApplyPunishment(punishment, user);

                if (punishmentResponse.Successful)
                {
                    activePunishments.Add(punishment);

                    if (punishmentMethodService.GetPunishmentMetaData().RemovesUserFromGuild)
                        break;
                }
            }

            await AddActivePunishments(activePunishments, userData);
        }

        private async Task AddActivePunishments(IEnumerable<Punishment> punishments, UserWarnings userData)
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

                foreach (var p in activePunishments)
                {
                    if (p.PunishmentExpires > DateTimeOffset.Now && p.PunishmentExpires != DateTimeOffset.MaxValue)
                    {
                       removePunishmentService.ScheduleActivePunishmentRemoval(p);
                    }
                }    
            }
        }

        private bool IsPunishmentAlreadyActive(Punishment punishment, UserWarnings userData)
        {
            var currentlyActivePunishments = userData.ActivePunishments;

            // bool indicating whether user already has punishment.
            return currentlyActivePunishments.Any(x => x.PunishmentId == punishment.Id);
        }

        private async Task RemoveActivePunishmentFromDb(ActivePunishment punishment)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var activePunishmentService = scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

                await activePunishmentService.Remove(punishment);
            }
        }

        public async Task RemovePunishment(ActivePunishment punishment)
        {
            await _punishmentFactory.GetPunishment(punishment.PunishType).RemovePunishment(punishment);

            await RemoveActivePunishmentFromDb(punishment);
        }

        public async Task RemovePunishmentBulk(List<ActivePunishment> punishments)
        {
            var tasks = new List<Task>();

            foreach (var p in punishments)
            {
                tasks.Add(Task.Run(() => RemovePunishment(p)));
            }

            await Task.WhenAll(tasks);
        }
    }
}
