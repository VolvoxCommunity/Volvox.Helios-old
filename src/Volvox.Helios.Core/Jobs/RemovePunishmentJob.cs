using System;
using System.Collections.Generic;
using System.Text;
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
            DiscordSocketClient client,
            ILogger<IModule> logger)
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

            var user = guild.Users.FirstOrDefault(x => x.Id == punishment.User.UserId);

            var role = guild.GetRole(punishment.RoleId.Value);

            await user.RemoveRoleAsync(role);
        }
    }
}
