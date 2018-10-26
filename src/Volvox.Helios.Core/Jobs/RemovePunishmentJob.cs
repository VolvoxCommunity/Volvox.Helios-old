using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
    public class RemovePunishmentJob
    {

        IJobService _jobService;
        IServiceScopeFactory _scopeFactory;
        IBot _bot;
        IModuleSettingsService<ModerationSettings> _moderationSettings;
        ILogger<IModule> _logger;

        public RemovePunishmentJob(IJobService jobService,
            IServiceScopeFactory scopeFactory,
            IBot bot, IModuleSettingsService<ModerationSettings> moderationSettings,
            ILogger<IModule> logger)
        {
            _jobService = jobService;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _bot = bot;
            _moderationSettings = moderationSettings;
        }

        public async Task SchedulePunishmentRemoval(ActivePunishment punishment)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var punishmentService = scope.ServiceProvider.GetRequiredService<IEntityService<ActivePunishment>>();

                await punishmentService.Remove(punishment);
            }
        }

        private async Task RemovePunishmentDiscord(ActivePunishment punishment)
        {
            switch (punishment.PunishType)
            {
                case ( PunishType.Ban ):
                    break;

                case ( PunishType.AddRole ):
                    break;
            }
        }

        private async Task Unban(ActivePunishment punishment)
        { 

        }

        private async Task RemoveRole(ActivePunishment punishment)
        {

        }
    }
}
