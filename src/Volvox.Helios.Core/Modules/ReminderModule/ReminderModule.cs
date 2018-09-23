using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.ModuleSettings;
using Volvox.Helios.Service.BackgroundJobs;
using Volvox.Helios.Service.Jobs;
using Volvox.Helios.Core.Bot;

namespace Volvox.Helios.Core.Modules.ReminderModule
{
    public class ReminderModule : Module
    {
        private readonly IJobService _jobService;
        private readonly IModuleSettingsService<ReminderSettings> _moduleSettings;
        private readonly EntityChangedDispatcher<RecurringReminderMessage> _entityChangedDispatcher;
        private readonly IServiceScopeFactory _scopeFactory;

        public ReminderModule(IDiscordSettings discordSettings,
            ILogger<IModule> logger,
            IConfiguration config,
            IJobService jobService,
            IModuleSettingsService<ReminderSettings> moduleSettings,
            EntityChangedDispatcher<RecurringReminderMessage> entityChangedDispatcher,
            IServiceScopeFactory scopeFactory)
            : base(discordSettings, logger, config)
        {
            _jobService = jobService;
            _moduleSettings = moduleSettings;
            _entityChangedDispatcher = entityChangedDispatcher;
            _scopeFactory = scopeFactory;
        }

        public override Task Init(DiscordSocketClient client)
        {
            _moduleSettings.SettingsChanged += OnModuleSettingsChanged;
            _entityChangedDispatcher.EntityCreated += OnReminderChanged;
            _entityChangedDispatcher.EntityUpdated += OnReminderChanged;
            _entityChangedDispatcher.EntityDeleted += OnReminderDeleted;
            client.ChannelDestroyed += OnChannelDestroyed;

            return Task.CompletedTask;
        }

        private async void OnModuleSettingsChanged(object sender, ModuleSettingsChangedArgs<ReminderSettings> args)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var entityService = scope.ServiceProvider.GetRequiredService<IEntityService<RecurringReminderMessage>>();
                var reminders = await entityService.Get(x => x.GuildId == args.Settings.GuildId);
                if (reminders is null)
                    return;

                if (args.Settings.Enabled)
                {
                    var reminderJob = scope.ServiceProvider.GetRequiredService<RecurringReminderMessageJob>();
                    foreach (var reminder in reminders)
                    {
                        _jobService.ScheduleRecurringJob(() => reminderJob.Run(reminder),
                            reminder.CronExpression,
                            reminder.JobId);
                    }
                }
                else
                {
                    foreach (var reminder in reminders)
                        _jobService.CancelRecurringJob(reminder.JobId);
                }
            }
        }

        private async Task OnChannelDestroyed(SocketChannel channel)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var reminderService = scope.ServiceProvider.GetRequiredService<IEntityService<RecurringReminderMessage>>();
                var existingReminders = await reminderService.GetAll(msg => msg.ChannelId == channel.Id);
                if (existingReminders is null || existingReminders.Count == 0)
                    return;

                var disableTasks = existingReminders
                    .Select(async rmd =>
                    {
                        rmd.Enabled = false;
                        await reminderService.Update(rmd);
                        _jobService.CancelRecurringJob(rmd.JobId);
                    });

                await Task.WhenAll(disableTasks);

                if (channel is SocketGuildChannel sgc)
                    Logger.LogInformation("{Count} reminders automatically removed for guild {GuildId}",
                        existingReminders.Count,
                        sgc.Guild.Id);
            }
        }

        private void OnReminderChanged(object sender, EntityChangedEventArgs<RecurringReminderMessage> args)
        {
            var reminder = args.Entity;
            using (var scope = _scopeFactory.CreateScope())
            {
                var reminderJob = scope.ServiceProvider.GetRequiredService<RecurringReminderMessageJob>();
                _jobService.ScheduleRecurringJob(() => reminderJob.Run(reminder),
                    reminder.CronExpression,
                    reminder.JobId);
            }
        }

        private void OnReminderDeleted(object sender, EntityChangedEventArgs<RecurringReminderMessage> args)
        {
            var reminder = args.Entity;
            _jobService.CancelRecurringJob(reminder.JobId);
        }
    }
}
