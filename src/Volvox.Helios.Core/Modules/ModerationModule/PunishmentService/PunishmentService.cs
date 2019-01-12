using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Core.Jobs;
using Volvox.Helios.Core.Modules.ModerationModule.Factories.PunishmentFactory;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.ActivePunishmentService;
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

        private readonly IActivePunishmentService _activePunishmentService;

        public PunishmentService(IServiceScopeFactory scopeFactory, IUserWarningsService userWarningService,
            IPunishmentFactory punishmentFactory, IActivePunishmentService activePunishmentService)
        {
            _scopeFactory = scopeFactory;

            _userWarningService = userWarningService;

            _punishmentFactory = punishmentFactory;

            _activePunishmentService = activePunishmentService;
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
                if (_activePunishmentService.IsPunishmentAlreadyActive(punishment, userData))
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

            await _activePunishmentService.AddActivePunishments(activePunishments, userData.Id);
        }

        public async Task RemovePunishment(ActivePunishment punishment)
        {
            await _punishmentFactory.GetPunishment(punishment.PunishType).RemovePunishment(punishment);

            await _activePunishmentService.RemoveActivePunishmentFromDb(punishment);
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
