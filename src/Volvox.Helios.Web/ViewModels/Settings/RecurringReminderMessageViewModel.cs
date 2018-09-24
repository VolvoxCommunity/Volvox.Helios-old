using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public enum RecurrenceType { Minutly, Hourly, Daily, Weekly, Monthly, Yearly }

    public class EditRecurringReminderMessageViewModel
    {
        public Guid Id { get; set; }
        public ulong GuildId { get; set; }

        [Required]
        [DisplayName("Reminder Message")]
        [DataType(DataType.MultilineText)]
        [MaxLength(500)]
        [Description("This is the message you would like to be sent every time this reminder is sent.")]
        public string Message { get; set; }

        [Required]
        [DisplayName("Available Channels")]
        public ulong ChannelId { get; set; }

        public string ChannelName { get; set; }

        public SelectList Channels { get; set; }

        [Required]
        [DefaultValue("*")]
        public string Minutes { get; set; }

        [Required]
        [DefaultValue("*")]
        public string Hours { get; set; }

        [Required]
        [DisplayName("Day of Month")]
        [DefaultValue("*")]
        public string DayOfMonth { get; set; }

        [Required]
        [DefaultValue("*")]
        public string Month { get; set; }

        [Required]
        [DisplayName("Day of Week")]
        [DefaultValue("*")]
        public string DayOfWeek { get; set; }

        [Required]
        public bool Enabled { get; set; }
    }
}
