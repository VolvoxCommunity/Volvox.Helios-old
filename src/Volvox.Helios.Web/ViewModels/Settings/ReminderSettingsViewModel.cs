using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class ReminderSettingsViewModel
    {
        public ulong GuildId { get; set; }

        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(false)]
        public bool Enabled { get; set; }

        public IEnumerable<RecurringReminderMessageViewModel> RecurringReminders { get; set; }
            = Enumerable.Empty<RecurringReminderMessageViewModel>();

        public static ReminderSettingsViewModel FromData(ReminderSettings settings)
        {
            if (settings is null)
                return new ReminderSettingsViewModel();

            return new ReminderSettingsViewModel
            {
                GuildId = settings.GuildId,
                Enabled = settings.Enabled,
                RecurringReminders = settings.RecurringReminders?.Select(x => new RecurringReminderMessageViewModel
                {
                    ChannelId = x.ChannelId,
                    Enabled = x.Enabled,
                    Id = x.Id,
                    Message = x.Message,
                }) ?? Enumerable.Empty<RecurringReminderMessageViewModel>()
            };
        }
    }
}
