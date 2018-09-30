using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Volvox.Helios.Domain.Module
{
    public class RecurringReminderMessage
    {
        public enum FaultType { None = 0, InvalidCron = 1, InvalidChannel = 2 }

        public Guid Id { get; set; }
        public ulong GuildId { get; set; }
        public bool Enabled { get; set; }
        public string Message { get; set; }
        public ulong ChannelId { get; set; }
        public string CronExpression { get; set; }
        public FaultType Fault  { get; set; }

        public string GetJobId()
        {
            return $"RRM:{Id.ToString()}";
        }
    }
}
