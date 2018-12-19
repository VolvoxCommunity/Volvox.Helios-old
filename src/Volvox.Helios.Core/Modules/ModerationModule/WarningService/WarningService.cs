using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Core.Modules.ModerationModule.WarningService
{
    public class WarningService : IWarningService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger<ModerationModule> _logger;

        public WarningService(IServiceScopeFactory scopeFactory, ILogger<ModerationModule> logger)
        {
            _scopeFactory = scopeFactory;

            _logger = logger;
        }

        public async Task AddWarning(ModerationSettings moderationSettings, SocketGuildUser user, UserWarnings userData, WarningType warningType)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var warningService = scope.ServiceProvider.GetRequiredService<IEntityService<Warning>>();

                var specificWarningDuration = GetWarningDuration(moderationSettings, warningType);

                var expireDate = DateTimeOffset.Now.AddMinutes(specificWarningDuration);

                // 0 means punishment lasts forever.
                if (specificWarningDuration == 0)
                    expireDate = DateTimeOffset.MaxValue;

                var warning = new Warning()
                {
                    UserId = userData.Id,
                    WarningRecieved = DateTimeOffset.Now,
                    WarningExpires = expireDate,
                    WarningType = warningType
                };

                if (userData.Warnings == null)
                    userData.Warnings = new List<Warning>();

                userData.Warnings.Add(warning);

                await warningService.Create(warning);

                _logger.LogInformation($"Moderation Module: User {user.Username} warned. Added warning to database. " +
                    $"Guild Id: {user.Guild.Id}, User Id: {user.Id}");
            }
        }

        private int GetWarningDuration(ModerationSettings moderationSettings, WarningType warningType)
        {
            var duration = 0;

            switch (warningType)
            {
                case ( WarningType.Link ):
                    duration = moderationSettings.LinkFilter.WarningExpirePeriod;
                    break;
                case ( WarningType.Profanity ):
                    duration = moderationSettings.ProfanityFilter.WarningExpirePeriod;
                    break;
            }

            return duration;
        }
    }
}
