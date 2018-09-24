using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.Discord;
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

        public IEnumerable<EditRecurringReminderMessageViewModel> RecurringReminders { get; set; }
            = Enumerable.Empty<EditRecurringReminderMessageViewModel>();

        public static ReminderSettingsViewModel FromData(RemembotSettings settings, IEnumerable<Channel> channels)
        {
            if (settings is null)
                return new ReminderSettingsViewModel();

            return new ReminderSettingsViewModel
            {
                GuildId = settings.GuildId,
                Enabled = settings.Enabled,
                RecurringReminders = settings.RecurringReminders?.Select(x => new EditRecurringReminderMessageViewModel
                {
                    Enabled = x.Enabled,
                    Id = x.Id,
                    Message = x.Message,
                    ChannelName = channels.FirstOrDefault(y => y.Id == x.ChannelId)?.Name ?? "NA"
                }) ?? Enumerable.Empty<EditRecurringReminderMessageViewModel>()
            };
        }
    }
}
