using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Jobs;
using Volvox.Helios.Core.Modules.ModerationModule.UserWarningsService;
using Volvox.Helios.Core.Services.MessageService;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService
{
    public class PunishmentService : IPunishmentService
    {
        private readonly IMessageService _messageService;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IUserWarningsService _userWarningService;

        private readonly DiscordSocketClient _client;

        private readonly ILogger<ModerationModule> _logger;

        public PunishmentService(IMessageService messageService, IServiceScopeFactory scopeFactory,
            DiscordSocketClient client, ILogger<ModerationModule> logger,
            IUserWarningsService userWarningService)
        {
            _messageService = messageService;

            _scopeFactory = scopeFactory;

            _userWarningService = userWarningService;

            _client = client;

            _logger = logger;
        }

        /// <inheritdoc />
        public async Task ApplyPunishments(ModerationSettings moderationSettings, ulong channelId, List<Punishment> punishments, SocketGuildUser user)
        {
            var userData = await _userWarningService.GetUser(user.Id, user.Guild.Id, u => u.ActivePunishments);

            var userHasBeenRemoved = false;

            // List of punishments to add to database as active punishments.
            var activePunishments = new List<Punishment>();

            foreach (var punishment in punishments)
            {
                // If a user has been kicked/banned or otherwise removed from the guild, you can't add any other punishments. So return from this method.
                if (userHasBeenRemoved)
                    return;

                // Check to make sure user doesn't already have this punishment. This could cause issues if the same punishment is applied twice.
                if (IsPunishmentAlreadyActive(punishment, userData))
                    continue;

                var wasSuccessful = true;

                // For each punishment, check if applying them has been successful. If successful, add this to active punishments db, if not, don't add it.
                // If a punishment removes the user from the guild, no more punisments can be applied to them, so don't attempt any more.
                switch (punishment.PunishType)
                {
                    case ( PunishType.Kick ):
                        if (await KickPunishment(punishment, channelId, user))
                            userHasBeenRemoved = true;
                        else
                            wasSuccessful = false;
                        break;

                    case ( PunishType.Ban ):
                        if (await BanPunishment(punishment, channelId, user))
                            userHasBeenRemoved = true;
                        else
                            wasSuccessful = false;
                        break;

                    case ( PunishType.AddRole ):
                        if (await AddRolePunishment(punishment, channelId, user))
                        { }
                        else
                            wasSuccessful = false;
                        break;
                }

                // Punishment applied successfully, add to list to add to db.
                if (wasSuccessful)
                    activePunishments.Add(punishment);

                wasSuccessful = true;
            }

            await AddActivePunishments(moderationSettings, activePunishments, user, userData);
        }

        private async Task<bool> AddRolePunishment(Punishment punishment, ulong channelId, SocketGuildUser user)
        {
            if (!punishment.RoleId.HasValue)
                return false;

            var guild = user.Guild;

            var role = guild.GetRole(punishment.RoleId.Value);

            if (guild is null)
                return false;

            if (role is null)
            {
                await _messageService.Post(channelId, $"Could not add Role to user '{user.Username}' as Role doesn't exist.");
                return false;
            }

            var hierarchy = _client.GetGuild(user.Guild.Id)?.CurrentUser.Hierarchy ?? 0;

            // Trying to assign a role higher than the bots hierarchy will throw an error.
            if (role.Position < hierarchy)
            {
                await user.AddRoleAsync(role);

                var expireTime = punishment.PunishDuration == 0 ? "Never" : punishment.PunishDuration.ToString();

                await _messageService.Post(channelId, $"Adding role '{role.Name}' to user <@{user.Id}>" +
                    $"\nReason: {punishment.WarningType}\n" +
                    $"Expires (minutes): {expireTime}");
            }
            else
            {
                _logger.LogInformation($"Moderation Module: Couldn't apply role '{role.Name}' to user '{user.Username}' as bot doesn't have appropriate permissions. " +
                    $"Guild Id:{user.Guild.Id}, Role Id: {punishment.RoleId.Value}, User Id: {user.Id}.");

                await _messageService.Post(channelId, $"Couldn't add role '{role.Name}' to user <@{user.Id}> as bot has insufficient permissions. " +
                    $"Check your role hierarchy and make sure the bot is higher than the role you wish to apply.");

                return false;
            }

            return true;
        }

        private async Task<bool> KickPunishment(Punishment punishment, ulong channelId, SocketGuildUser user)
        {
            var botHierarchy = _client.GetGuild(user.Guild.Id)?.CurrentUser.Hierarchy ?? 0;

            if (botHierarchy > user.Hierarchy)
            {
                await user.KickAsync();

                _logger.LogInformation($"Moderation Module: Kicking user '{user.Username}'." +
                        $"Guild Id:{user.Guild.Id}, User Id: {user.Id}.");

                await _messageService.Post(channelId, $"Kicking user <@{user.Id}>" +
                    $"\nReason: {punishment.WarningType}");

                return true;
            }
            else
            {
                _logger.LogInformation($"Moderation Module: Failed to kick user '{user.Username}' as bot has insufficient permissions." +
                        $"Guild Id:{user.Guild.Id}, User Id: {user.Id}.");

                await _messageService.Post(channelId, $"Failed to kick user <@{user.Id}> as bot has insufficient permissions.");

                return false;
            }
        }

        private async Task<bool> BanPunishment(Punishment punishment, ulong channelId, SocketGuildUser user)
        {
            var botHierarchy = _client.GetGuild(user.Guild.Id)?.CurrentUser.Hierarchy ?? 0;

            if (botHierarchy > user.Hierarchy)
            {
                await user.Guild.AddBanAsync(user);

                _logger.LogInformation($"Moderation Module: Banning user '{user.Username}'." +
                        $"Guild Id:{user.Guild.Id}, User Id: {user.Id}.");

                var expireTime = punishment.PunishDuration == 0 ? "Never" : punishment.PunishDuration.ToString();

                await _messageService.Post(channelId, $"Banning user <@{user.Id}>" +
                    $"\nReason: {punishment.WarningType}" +
                    $"Expires: {expireTime}");

                return true;
            }
            else
            {
                _logger.LogInformation($"Moderation Module: Failed to ban user '{user.Username}' as bot has insufficient permissions." +
                        $"Guild Id:{user.Guild.Id}, User Id: {user.Id}.");

                await _messageService.Post(channelId, $"Failed to ban user <@{user.Id}> as bot has insufficient permissions.");

                return false;
            }
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

                await activePunishmentsService.CreateBulk(activePunishments);

                // Schedule punishment removals where punishments expire. No point in scheduling punishments for removal if they never expire.
                await removePunishmentService.SchedulePunishmentRemovals(activePunishments.Where(x => x.PunishmentExpires > DateTimeOffset.Now));
            }
        }

        private bool IsPunishmentAlreadyActive(Punishment punishment, UserWarnings userData)
        {
            var currentlyActivePunishments = userData.ActivePunishments;

            // bool indicating whether user already has punishment.
            return ( currentlyActivePunishments.Any(x => x.PunishmentId == punishment.Id) );
        }
    }
}
