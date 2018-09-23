using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Volvox.Helios.Domain.Module
{
    public class RecurringReminderMessage
    {
        public ulong Id { get; set; }
        public ulong GuildId { get; set; }
        public string JobId { get; set; }
        public bool Enabled { get; set; }
        public string Message { get; set; }
        public ulong ChannelId { get; set; }
        public string ReadableCronExpression { get; set; }
        public string CronExpression { get; set; }
    }
}
