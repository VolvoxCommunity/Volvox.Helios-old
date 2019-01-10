using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Jobs;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments;
using Volvox.Helios.Core.Modules.ModerationModule.UserWarningsService;
using Volvox.Helios.Core.Modules.ModerationModule.Utils;
using Volvox.Helios.Core.Services.MessageService;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.BackgroundJobs;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService
{
    // TODO : Maybe make this service scope or transient?

    // TODO : initially profanity filter failed.the fix was to go to filter and save. why is that? caching maybe? or maybe the function to create if not exists in the controller doesnt update the cache properly, then saving updates it as the cache is cleared?

    public class PunishmentService : IPunishmentService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IUserWarningsService _userWarningService;

        private readonly IModerationModuleUtils _moderationModuleUtils;

        private readonly Dictionary<PunishType, IPunishment> _punishments = new Dictionary<PunishType, IPunishment>();

        public PunishmentService(IServiceScopeFactory scopeFactory, IUserWarningsService userWarningService,
            IList<IPunishment> punishments, IModerationModuleUtils moderationModuleUtils)
        {
            _scopeFactory = scopeFactory;

            _userWarningService = userWarningService;

            _moderationModuleUtils = moderationModuleUtils;

            foreach (var punishment in punishments)
            {
                var punishmentType = punishment.GetPunishmentTypeDetails().PunishType;

                _punishments[punishmentType] = punishment;
            }
        }

        /// <inheritdoc />
        public async Task ApplyPunishments(List<Punishment> punishments, SocketGuildUser user)
        {
            var settings = await _moderationModuleUtils.GetModerationSettings(user.Guild.Id);

            var userData = await _userWarningService.GetUser(user.Id, user.Guild.Id, u => u.ActivePunishments);

            // List of punishments to add to database as active punishments.
            var activePunishments = new List<Punishment>();

            foreach (var punishment in punishments)
            {
                // Check to make sure user doesn't already have this punishment. This could cause issues if the same punishment is applied twice.
                if (IsPunishmentAlreadyActive(punishment, userData))
                    continue;

                var punishmentResponse = await _punishments[punishment.PunishType].ApplyPunishment(punishment, user);

                if (punishmentResponse.Successful)
                {
                    activePunishments.Add(punishment);

                    // If the punishment was successful, and this type of punishment removes the user from the guild, do nothing.
                    if (_punishments[punishment.PunishType].GetPunishmentTypeDetails().RemovesUserFromGuild)
                        break;
                }
            }

            await AddActivePunishments(settings, activePunishments, user, userData);
        }

        private async Task AddActivePunishments(ModerationSettings moderationSettings, List<Punishment> punishments, SocketGuildUser user, UserWarnings userData)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var activePunishmentsService = scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

                var userWarningsService = scope.ServiceProvider.GetRequiredService<IEntityService<UserWarnings>>();

                var activePunishments = new List<ActivePunishment>();

                foreach (var punishment in punishments)
                {
                    // No need to add kick punishment to DB as it's not a punishment with a duration.
                    if (punishment.PunishType == PunishType.Kick)
                        continue;

                    // If PunishDuration is 0, user never wants punishment to expire.
                    var expireDate = punishment.PunishDuration == 0
                        ? DateTimeOffset.MaxValue
                        : DateTimeOffset.Now.AddMinutes(punishment.PunishDuration);

                    // Refetching prevents self referencing loop AND ensures the entity is tracked, therefore stopping ef core from trying to re-add it.
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
            return ( currentlyActivePunishments.Any(x => x.PunishmentId == punishment.Id) );
        }

        private async Task RemoveActivePunishmentFromDb(ActivePunishment punishment)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var activePunishmentService = scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

                await activePunishmentService.Remove(punishment);
            }
        }

        private bool HasSufficientPrivilages(SocketRole role, SocketGuild guild)
        {
            var hierarchy = guild.CurrentUser.Hierarchy;

            return hierarchy > role.Position;
        }

        public async Task RemovePunishment(ActivePunishment punishment)
        {
            await _punishments[punishment.PunishType].RemovePunishment(punishment);

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
