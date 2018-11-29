using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.BackgroundJobs;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Jobs
{
    //****************************************************************************
    //***********  TODO : NULL CHECKS, ESPECIALLY INSIDE REMOVE ROLE METHOD ******
    //****************************************************************************

    public class RemovePunishmentJob
    {

        IJobService _jobService;
        IServiceScopeFactory _scopeFactory;
        IBot _bot;
        IModuleSettingsService<ModerationSettings> _moderationSettings;
        ILogger<IModule> _logger;
        private readonly DiscordSocketClient _client;

        public RemovePunishmentJob(IJobService jobService,
            IServiceScopeFactory scopeFactory,
            IBot bot, IModuleSettingsService<ModerationSettings> moderationSettings,
            DiscordSocketClient client, ILogger<IModule> logger)
        {
            _jobService = jobService;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _bot = bot;
            _moderationSettings = moderationSettings;
            _client = client;
        }

        public async Task SchedulePunishmentRemovals(IEnumerable<ActivePunishment> punishments)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                // TODO: In each method, create activepunishment service to remove punishments from db.
                // TODO: null checks in methods
                foreach (var punishment in punishments)
                {
                    _jobService.ScheduleJob(() => RemovePunishmentDiscord(punishment), punishment.PunishmentExpires);
                }
            }
        }

        public async Task RemovePunishmentDiscord(ActivePunishment punishment)
        {
            switch (punishment.PunishType)
            {
                case ( PunishType.Ban ):
                    await Unban(punishment);
                    break;

                case ( PunishType.AddRole ):
                    await RemoveRole(punishment);
                    break;
            }

            await RemoveActivePunishmentFromDb(punishment);
        }

        private async Task Unban(ActivePunishment punishment)
        {
            var guild = _client.GetGuild(punishment.User.GuildId);

            var userId = punishment.User.UserId;

            await guild.RemoveBanAsync(userId);
        }

        private async Task RemoveRole(ActivePunishment punishment)
        {
            var guild = _client.GetGuild(punishment.User.GuildId);

            var role = guild?.GetRole(punishment.RoleId.Value);

            var user = guild?.Users.FirstOrDefault(x => x.Id == punishment.User.UserId);

            // If role hierarchy has changed since punishment was applied and now the bot doesn't have sufficient privilages, do nothing.
            if (role != null && HasSufficientPrivilages(role, guild))
            {
                await user.RemoveRoleAsync(role);

                _logger.LogInformation($"Moderation Module: Removing role '{role.Name}' from {user.Username}."  +
                    $"Guild Id:{user.Guild.Id}, Role Id: {punishment.RoleId.Value}, User Id: {user.Id}.");
            }
            else
            {
                if (user != null && guild != null)
                {
                    _logger.LogInformation($"Moderation Module: Couldn't apply role '{role.Name}' to user {user.Username} as bot doesn't have appropriate permissions. " +
                       $"Guild Id:{guild.Id}, Role Id: {punishment.RoleId.Value}, User Id: {user.Id}");
                }
                else
                {
                    _logger.LogError("Moderation Module: Something wen't wrong when trying to remove role in the punishment removal job. User or guild were unexpectedly null.");
                }
            }
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
    }
}
