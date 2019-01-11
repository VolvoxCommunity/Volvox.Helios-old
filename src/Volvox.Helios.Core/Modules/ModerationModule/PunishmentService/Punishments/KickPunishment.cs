using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.PunishmentService.Punishments
{
    public class KickPunishment : IPunishment
    {
        public async Task<PunishmentResponse> ApplyPunishment(Punishment punishment, SocketGuildUser user)
        {
            var punishmentResponse = new PunishmentResponse();

            var botHierarchy = user.Guild.CurrentUser.Hierarchy;

            if (botHierarchy > user.Hierarchy)
            {
                await user.KickAsync();

                punishmentResponse.Successful = true;

                punishmentResponse.StatusMessage = $"Kicking user <@{user.Id}>" +
                    $"\nReason: {punishment.WarningType}";
            }
            else
            {
                punishmentResponse.StatusMessage = $"Failed to kick user <@{user.Id}> as bot has insufficient permissions.";
            }

            return punishmentResponse;
        }

        public PunishmentMetaData GetPunishmentMetaData()
        {
            return new PunishmentMetaData
            {
                PunishType = PunishType.Kick,
                RemovesUserFromGuild = true
            };
        }

        public Task<PunishmentResponse> RemovePunishment(ActivePunishment punishment)
        {
            return Task.FromResult(new PunishmentResponse { Successful = true });
        }
    }
}
