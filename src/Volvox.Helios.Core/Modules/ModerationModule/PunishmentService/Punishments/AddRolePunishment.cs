using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments
{
    public class AddRolePunishment : IPunishment
    {
        private readonly DiscordSocketClient _client;

        public AddRolePunishment(DiscordSocketClient client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public async Task<PunishmentResponse> ApplyPunishment(Punishment punishment, SocketGuildUser user)
        {
            var punishmentResponse = new PunishmentResponse();

            if (!punishment.RoleId.HasValue)
            {
                punishmentResponse.Successful = false;
                punishmentResponse.StatusMessage = "No role provided.";
                return punishmentResponse;
            }

            var guild = user.Guild;

            var role = guild.GetRole(punishment.RoleId.Value);

            if (guild == null)
            {
                punishmentResponse.Successful = false;
                punishmentResponse.StatusMessage = "Guild doesn't exist.";
                return punishmentResponse;
            }

            if (role == null)
            {
                punishmentResponse.Successful = false;
                punishmentResponse.StatusMessage = "Role no longer exists.";
                return punishmentResponse;
            }

            var hierarchy = user.Guild.CurrentUser.Hierarchy;

            // Trying to assign a role higher than the bots hierarchy will throw an error.
            if (role.Position < hierarchy)
            {
                await user.AddRoleAsync(role);

                var expireTime = punishment.PunishDuration == 0 ? "Never" : punishment.PunishDuration.ToString();

                punishmentResponse.Successful = true;

                punishmentResponse.StatusMessage = $"Adding role '{role.Name}' to user <@{user.Id}>" +
                                                   $"\nReason: {punishment.WarningType}\n" +
                                                   $"Expires (minutes): {expireTime}";
            }
            else
            {
                punishmentResponse.Successful = false;

                punishmentResponse.StatusMessage =
                    $"Couldn't add role '{role.Name}' to user <@{user.Id}> as bot has insufficient permissions. " +
                    "Check your role hierarchy and make sure the bot is higher than the role you wish to apply.";
            }

            return punishmentResponse;
        }

        /// <inheritdoc />
        public PunishmentMetaData GetPunishmentMetaData()
        {
            return new PunishmentMetaData
            {
                PunishType = PunishType.AddRole,
                RemovesUserFromGuild = false
            };
        }

        /// <inheritdoc />
        public async Task<PunishmentResponse> RemovePunishment(ActivePunishment punishment)
        {
            var punishmentResponse = new PunishmentResponse();

            var guild = _client.GetGuild(punishment.User.GuildId);

            var role = guild?.GetRole(punishment.RoleId.Value);

            var user = guild?.Users.FirstOrDefault(x => x.Id == punishment.User.UserId);

            // If role hierarchy has changed since punishment was applied and now the bot doesn't have sufficient privileges, do nothing.
            if (role != null && HasSufficientPrivileges(role, guild))
            {
                await user.RemoveRoleAsync(role);

                punishmentResponse.Successful = true;

                punishmentResponse.StatusMessage = "";
            }
            else
            {
                punishmentResponse.Successful = false;
                if (user != null)
                    punishmentResponse.StatusMessage =
                        $"Moderation Module: Couldn't apply role '{role?.Name ?? "role"}' to user {user.Username} as bot doesn't have appropriate permissions. " +
                        $"Guild Id:{guild.Id}, Role Id: {punishment.RoleId.Value}, User Id: {user.Id}";
                else
                    punishmentResponse.StatusMessage =
                        "Moderation Module: Something went wrong when trying to remove role in the punishment removal job. User was unexpectedly null.";
            }

            return punishmentResponse;
        }

        private bool HasSufficientPrivileges(SocketRole role, SocketGuild guild)
        {
            var hierarchy = guild.CurrentUser.Hierarchy;

            return hierarchy > role.Position;
        }
    }
}