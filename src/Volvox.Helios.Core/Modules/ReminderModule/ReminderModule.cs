using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.User;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Service.Jobs;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ReminderModule
{
    public class ReminderModule : Module
    {
        private readonly IJobService _jobService;
        private readonly IEntityService<RecurringReminderMessage> _reminderService;
        private readonly IModuleSettingsService<ReminderSettings> _moduleSettings;
        private static DiscordSocketClient _socketClient;

        public ReminderModule(IDiscordSettings discordSettings,
            ILogger<IModule> logger,
            IConfiguration config,
            IJobService jobService,
            IEntityService<RecurringReminderMessage> reminderService,
            IModuleSettingsService<ReminderSettings> moduleSettings)
            : base(discordSettings, logger, config)
        {
            _jobService = jobService;
            _moduleSettings = moduleSettings;
            _reminderService = reminderService;
        }

        public override Task Init(DiscordSocketClient client)
        {
            _socketClient = client;

            _moduleSettings.SettingsChanged += OnModuleSettingsChanged;
            _reminderService.Dispatch.EntityCreated += OnReminderChanged;
            _reminderService.Dispatch.EntityUpdated += OnReminderChanged;
            _reminderService.Dispatch.EntityDeleted += OnReminderDeleted;
            client.ChannelDestroyed += OnChannelDestroyed;

            return Task.CompletedTask;
        }

        private void OnModuleSettingsChanged(object sender, ModuleSettingsChangedArgs<ReminderSettings> args)
        {
            if (args.Settings.Enabled)
            {
                foreach (var reminder in args.Settings.RecurringReminders)
                {
                    _jobService.ScheduleRecurringJob(() => SendReminderMessage(reminder, _jobService, _reminderService, Logger),
                        reminder.CronExpression,
                        reminder.JobId);
                }
            }
            else
            {
                foreach (var reminder in args.Settings.RecurringReminders)
                    _jobService.CancelRecurringJob(reminder.JobId);
            }
        }

        private async Task OnChannelDestroyed(SocketChannel channel)
        {
            var existingReminders = await _reminderService.GetAll(msg => msg.ChannelId == channel.Id);
            if (existingReminders is null || existingReminders.Count == 0)
                return;

            var disableTasks = existingReminders
                .Select(async rmd =>
                {
                    rmd.Enabled = false;
                    await _reminderService.Update(rmd);
                    _jobService.CancelRecurringJob(rmd.JobId);
                });

            await Task.WhenAll(disableTasks);

            if (channel is SocketGuildChannel sgc)
                Logger.LogInformation("{Count} reminders automatically removed for guild {GuildId}",
                    existingReminders.Count,
                    sgc.Guild.Id);
        }

        private void OnReminderChanged(object sender, EntityChangedEventArgs<RecurringReminderMessage> args)
        {
            var reminder = args.Entity;
            _jobService.ScheduleRecurringJob(() => SendReminderMessage(reminder, _jobService, _reminderService, Logger),
                reminder.CronExpression,
                reminder.JobId);
        }

        private void OnReminderDeleted(object sender, EntityChangedEventArgs<RecurringReminderMessage> args)
        {
            var reminder = args.Entity;
            _jobService.CancelRecurringJob(reminder.JobId);
        }

        public static async Task SendReminderMessage(
            RecurringReminderMessage reminder,
            IJobService jobService,
            IEntityService<RecurringReminderMessage> entityService,
            ILogger<IModule> logger)
        {
            var channel = _socketClient.GetChannel(reminder.ChannelId);
            if (channel is null)
            {
                logger.LogWarning("Attempt was made to send reminder to channel {ChannelId} that doesn't exist. Removing reminder and stopping job for guild {GuildId}.",
                    reminder.ChannelId,
                    reminder.GuildId);

                await DisableJob();
                return;
            }

            if(channel is SocketTextChannel stc)
            {
                await stc.SendMessageAsync(reminder.Message);
            }
            else
            {
                logger.LogInformation("Attempt was made to send reminder to a non-text channel {ChannelId}. Disabling reminder for guild {GuildId}.",
                    reminder.ChannelId,
                    reminder.GuildId);

                await DisableJob();
            }

            async Task DisableJob()
            {
                var trackedEntity = await entityService.Find(reminder.Id);
                trackedEntity.Enabled = false;
                await entityService.Update(trackedEntity);
                jobService.CancelRecurringJob(reminder.JobId);
            }
        }
    }
}
