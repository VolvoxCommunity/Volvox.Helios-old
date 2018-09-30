using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.Discord;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.Extensions;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class ReminderSettingsViewModel
    {
        public ulong GuildId { get; set; }

        [Required]
        [DisplayName("Enabled")]
        [DefaultValue(false)]
        public bool Enabled { get; set; }

        public IEnumerable<ReminderListItemViewModel> RecurringReminders { get; set; }
            = Enumerable.Empty<ReminderListItemViewModel>();

        public static ReminderSettingsViewModel FromData(RemembotSettings settings, IEnumerable<Channel> channels)
        {
            if (settings is null)
                return new ReminderSettingsViewModel();

            return new ReminderSettingsViewModel
            {
                GuildId = settings.GuildId,
                Enabled = settings.Enabled,
                RecurringReminders = settings.RecurringReminders?.Select(x =>
                {
                    var (isFaulted, status) = GetEnabledStatus(x);
                    var vm = new ReminderListItemViewModel
                    {
                        Id = x.Id,
                        Status = status,
                        IsFaulted = isFaulted,
                        Message = x.Message,
                        ChannelName = channels.FirstOrDefault(y => y.Id == x.ChannelId)?.Name ?? "NA"
                    };

                    return vm;
                })
            };
        }

        static (bool HasFault, string Status) GetEnabledStatus(RecurringReminderMessage reminderMessage)
        {
            if(reminderMessage.Fault == RecurringReminderMessage.FaultType.None)
            {
                if (reminderMessage.Enabled)
                    return (false, "Enabled");

                return (false, "Disabled");
            }
            else
            {
                return (true, reminderMessage.GetFaultShortMessage());
            }
        }
    }
}
