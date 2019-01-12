using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments
{
    public class BanPunishment : IPunishment
    {
        private readonly DiscordSocketClient _client;

        public BanPunishment(DiscordSocketClient client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public async Task<PunishmentResponse> ApplyPunishment(Punishment punishment, SocketGuildUser user)
        {
            var punishmentResponse = new PunishmentResponse();

            var botHierarchy = user.Guild.CurrentUser.Hierarchy;

            if (botHierarchy > user.Hierarchy)
            {
                await user.Guild.AddBanAsync(user);

                var expireTime = punishment.PunishDuration == 0 ? "Never" : punishment.PunishDuration.ToString();

                punishmentResponse.Successful = true;

                punishmentResponse.StatusMessage = $"Banning user <@{user.Id}>" +
                    $"\nReason: {punishment.WarningType}" +
                    $"Expires: {expireTime}";

            }
            else
            {
                punishmentResponse.Successful = false;

                punishmentResponse.StatusMessage = $"Failed to ban user <@{user.Id}> as bot has insufficient permissions.";
            }

            return punishmentResponse;
        }

        /// <inheritdoc />
        public PunishmentMetaData GetPunishmentMetaData()
        {
            return new PunishmentMetaData
            {
                PunishType = PunishType.Ban,
                RemovesUserFromGuild = true
            };
        }

        /// <inheritdoc />
        public async Task<PunishmentResponse> RemovePunishment(ActivePunishment punishment)
        {
            var guild = _client.GetGuild(punishment.User.GuildId);

            var userId = punishment.User.UserId;

            await guild.RemoveBanAsync(userId);

            return new PunishmentResponse { Successful = true };
        }
    }
}
