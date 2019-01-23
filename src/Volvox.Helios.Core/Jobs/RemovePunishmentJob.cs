using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Service.BackgroundJobs;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Core.Jobs
{
    public class RemovePunishmentJob
    {
        private readonly DiscordSocketClient _client;
        private readonly IJobService _jobService;
        private readonly IPunishmentService _punishmentService;
        private readonly IServiceScopeFactory _scopeFactory;

        public RemovePunishmentJob(IJobService jobService,
            IServiceScopeFactory scopeFactory,
            IPunishmentService punishmentService, DiscordSocketClient client)
        {
            _jobService = jobService;
            _scopeFactory = scopeFactory;
            _punishmentService = punishmentService;
            _client = client;
        }

        public string ScheduleActivePunishmentRemoval(ActivePunishment activePunishment)
        {
            return _jobService.ScheduleJob(() => RemoveActivePunishmentDiscord(activePunishment.Id),
                activePunishment.PunishmentExpires);
        }

        public async Task RemoveActivePunishmentDiscord(int activePunishmentId)
        {
            ActivePunishment activePunishment;

            while (_client.ConnectionState != ConnectionState.Connected) await Task.Delay(5000);

            using (var scope = _scopeFactory.CreateScope())
            {
                var activePunishmentService =
                    scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

                activePunishment = await activePunishmentService.GetFirst(p => p.Id == activePunishmentId, p => p.User);
            }

            // Active Punishment has already been removed by guild admin.
            if (activePunishment == null) return;

            await _punishmentService.RemovePunishment(activePunishment);
        }
    }
}