using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.BackgroundJobs;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;

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

        public string SchedulePunishmentRemoval(ActivePunishment punishment)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                return _jobService.ScheduleJob(() => RemovePunishmentDiscord(punishment), punishment.PunishmentExpires);
            }
        }

        public async Task RemovePunishmentDiscord(ActivePunishment punishment)
        {
            await _punishmentService.RemovePunishment(punishment);
        }
    }
}
