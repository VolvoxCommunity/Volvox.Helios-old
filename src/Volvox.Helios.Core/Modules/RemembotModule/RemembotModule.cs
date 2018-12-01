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
using Hangfire;
using System;

namespace Volvox.Helios.Core.Modules.ReminderModule
{
    /// <summary>
    /// Add and remove periodic reminder messages to a specific channel.
    /// </summary>
    public class RemembotModule : Module
    {
        private readonly IJobService _jobService;
        private readonly IModuleSettingsService<RemembotSettings> _moduleSettings;
        private readonly EntityChangedDispatcher<RecurringReminderMessage> _entityChangedDispatcher;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="discordSettings"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="jobService"></param>
        /// <param name="moduleSettings"></param>
        /// <param name="entityChangedDispatcher"></param>
        /// <param name="scopeFactory"></param>
        public RemembotModule(IDiscordSettings discordSettings,
            ILogger<IModule> logger,
            IConfiguration config,
            IJobService jobService,
            IModuleSettingsService<RemembotSettings> moduleSettings,
            EntityChangedDispatcher<RecurringReminderMessage> entityChangedDispatcher,
            IServiceScopeFactory scopeFactory)
            : base(discordSettings, logger, config)
        {
            _jobService = jobService;
            _moduleSettings = moduleSettings;
            _entityChangedDispatcher = entityChangedDispatcher;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        ///     Returns true if the module is enabled for the specified guild and false if not.
        /// </summary>
        /// <param name="guildId">Id if the guild to check.</param>
        /// <returns>True if the module is enabled for the specified guild and false if not.</returns>
        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            return ( await _moduleSettings.GetSettingsByGuild(guildId) ).Enabled;
        }

        /// <summary>
        ///     Initializes this module and subscribes to settings and reminder data changes to update the recurring jobs accordingly.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public override Task Init(DiscordSocketClient client)
        {
            _moduleSettings.SettingsChanged += OnModuleSettingsChanged;
            _entityChangedDispatcher.EntityCreated += OnReminderChanged;
            _entityChangedDispatcher.EntityUpdated += OnReminderChanged;
            _entityChangedDispatcher.EntityDeleted += OnReminderDeleted;
            client.ChannelDestroyed += OnChannelDestroyed;

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Handler for receiving settings changes for this module. Will start or cancel all reminder jobs based on the "Enabled" property.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnModuleSettingsChanged(object sender, ModuleSettingsChangedArgs<RemembotSettings> args)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var entityService = scope.ServiceProvider.GetRequiredService<IEntityService<RecurringReminderMessage>>();
                var reminders = await entityService.Get(x => x.GuildId == args.Settings.GuildId);
                if (reminders is null)
                    return;

                if (args.Settings.Enabled)
                {
                    foreach (var reminder in reminders.Where(x => x.Enabled))
                        StartOrUpdateJob(reminder);

                    foreach (var reminder in reminders.Where(x => !x.Enabled))
                        StopJobIfExists(reminder);
                }
                else
                {
                    foreach (var reminder in reminders)
                        StopJobIfExists(reminder);
                }
            }
        }

        /// <summary>
        ///     Handler for channels that are destroyed in a guild. Will cancel all reminder jobs for that channel as well as setting "Enabled" to false on each reminder.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private async Task OnChannelDestroyed(SocketChannel channel)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var reminderService = scope.ServiceProvider.GetRequiredService<IEntityService<RecurringReminderMessage>>();
                var existingReminders = await reminderService.GetAll(msg => msg.ChannelId == channel.Id);
                if (existingReminders is null || existingReminders.Count == 0)
                    return;

                var disableTasks = existingReminders
                    .Where(x => x.Fault == RecurringReminderMessage.FaultType.None)
                    .Select(async rmd =>
                    {
                        rmd.Enabled = false;
                        rmd.Fault = RecurringReminderMessage.FaultType.InvalidChannel;
                        await reminderService.Update(rmd);
                    });

                await Task.WhenAll(disableTasks);

                if (channel is SocketGuildChannel sgc)
                    Logger.LogInformation("{Count} reminders automatically removed for guild {GuildId}",
                        existingReminders.Count,
                        sgc.Guild.Id);
            }
        }

        /// <summary>
        ///     Handler for <see cref="RecurringReminderMessage"/>s that have either been created or updated. Checks the "Enabled" property to start or stop the recurring job.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnReminderChanged(object sender, EntityChangedEventArgs<RecurringReminderMessage> args)
        {
            var reminder = args.Entity;

            // Don't do anything with faulted reminders. User needs to correct
            // the fault before reminders can continue.
            if (reminder.Fault != RecurringReminderMessage.FaultType.None)
                return;

            var settings = await _moduleSettings.GetSettingsByGuild(args.Entity.GuildId);
            if (settings.Enabled)
            {
                if (reminder.Enabled)
                    StartOrUpdateJob(reminder);
                else
                    StopJobIfExists(reminder);
            }
            else
            {
                StopJobIfExists(reminder);
            }
        }

        /// <summary>
        ///     Handler for <see cref="RecurringReminderMessage"/>s that have been deleted. Will stop the recurring job associated with it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnReminderDeleted(object sender, EntityChangedEventArgs<RecurringReminderMessage> args)
        {
            var reminder = args.Entity;
            StopJobIfExists(reminder);
        }

        /// <summary>
        ///     Helper method to start or update the recurring job.
        /// </summary>
        /// <param name="reminder"></param>
        private void StartOrUpdateJob(RecurringReminderMessage reminder)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var reminderJob = scope.ServiceProvider.GetRequiredService<RecurringReminderMessageJob>();
                try
                {
                    _jobService.ScheduleRecurringJob(() => reminderJob.Run(reminder),
                        reminder.CronExpression,
                        reminder.GetJobId());
                }
                catch (ArgumentException)
                {
                    var reminderService = scope.ServiceProvider.GetRequiredService<IEntityService<RecurringReminderMessage>>();
                    reminder.Fault = RecurringReminderMessage.FaultType.InvalidCron;
                    reminderService.Update(reminder);
                }
            }
        }

        /// <summary>
        ///     Helper method to stop the recurring job if it exists.
        /// </summary>
        /// <param name="reminder"></param>
        private void StopJobIfExists(RecurringReminderMessage reminder)
        {
            _jobService.CancelRecurringJob(reminder.GetJobId());
        }
    }
}
