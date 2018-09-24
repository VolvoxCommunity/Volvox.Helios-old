using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Service.BackgroundJobs;
using Volvox.Helios.Service.EntityService;

namespace Volvox.Helios.Service.Jobs
{
    public class RecurringReminderMessageJob
    {
        IJobService _jobService;
        IServiceScopeFactory _scopeFactory;
        IBot _bot;
        ILogger<IModule> _logger;

        public RecurringReminderMessageJob(IJobService jobService,
            IServiceScopeFactory scopeFactory,
            IBot bot,
            ILogger<IModule> logger)
        {
            _jobService = jobService;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _bot = bot;
        }

        public async Task Run(RecurringReminderMessage reminder)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var channel = _bot.Client.GetChannel(reminder.ChannelId);
                if (channel is null)
                {
                    _logger.LogWarning("Attempt was made to send reminder to channel {ChannelId} that doesn't exist. Removing reminder and stopping job for guild {GuildId}.",
                        reminder.ChannelId,
                        reminder.GuildId);

                    await DisableJob();
                    return;
                }

                if (channel is SocketTextChannel stc)
                {
                    await stc.SendMessageAsync(reminder.Message);
                }
                else
                {
                    _logger.LogInformation("Attempt was made to send reminder to a non-text channel {ChannelId}. Disabling reminder for guild {GuildId}.",
                        reminder.ChannelId,
                        reminder.GuildId);

                    await DisableJob();
                }

                async Task DisableJob()
                {
                
                        var entityService = scope.ServiceProvider.GetRequiredService<IEntityService<RecurringReminderMessage>>();
                        var trackedEntity = await entityService.Find(reminder.Id);
                        trackedEntity.Enabled = false;
                        await entityService.Update(trackedEntity);
                        _jobService.CancelRecurringJob(reminder.GetJobId());
                
                }
            }
        }
    }
}
