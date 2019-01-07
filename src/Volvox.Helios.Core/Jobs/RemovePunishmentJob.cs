using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Service.BackgroundJobs;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Core.Jobs
{
    public class RemovePunishmentJob
    {
        private readonly IJobService _jobService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPunishmentService _punishmentService;

        public RemovePunishmentJob(IJobService jobService,
            IServiceScopeFactory scopeFactory,
            IPunishmentService punishmentService )
        {
            _jobService = jobService;
            _scopeFactory = scopeFactory;
            _punishmentService = punishmentService;
        }

        public string ScheduleActivePunishmentRemoval(ActivePunishment activePunishment)
        {
            return _jobService.ScheduleJob(() => RemoveActivePunishmentDiscord(activePunishment.Id), activePunishment.PunishmentExpires);
        }

        public async Task RemoveActivePunishmentDiscord(int activePunishmentId)
        {
            ActivePunishment activePunishment;
            
            using (var scope = _scopeFactory.CreateScope())
            {
                var activePunishmentService = scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

                activePunishment = await activePunishmentService.GetFirst(p => p.Id == activePunishmentId, p => p.User);
            }

            // Active Punishment has already been removed by guild admin.
            if (activePunishment == null) return;

            await _punishmentService.RemovePunishment(activePunishment);
        }
    }
}



