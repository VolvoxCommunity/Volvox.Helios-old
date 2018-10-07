using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.Settings
{
    public class EditRecurringReminderMessageViewModel
    {
        public long Id { get; set; }

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

        public int FaultType { get; set; }

        public string FaultMessage { get; set; }

        [Required]
        public string CronExpression { get; set; }

        [Required]
        public bool Enabled { get; set; }
    }
}
