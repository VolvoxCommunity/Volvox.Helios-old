using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public enum RecurrenceType { Minutly, Hourly, Daily, Weekly, Monthly, Yearly }

    public class RecurringReminderMessageViewModel
    {
        public ulong Id { get; set; }

        [Required]
        [DisplayName("ReminderName")]
        [Description("Sets an identifying name for this reminder.")]
        [MaxLength(120)]
        public string Name { get; set; }

        public string Message { get; set; }

        public DateTimeOffset PostDate { get; set; }
        public ulong ChannelId { get; set; }
        public string Recurrence { get; set; }
        public bool Enabled { get; set; }
    }
}
