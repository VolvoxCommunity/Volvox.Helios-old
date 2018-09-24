using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Volvox.Helios.Domain.Module
{
    public class RecurringReminderMessage
    {
        public Guid Id { get; set; }
        public ulong GuildId { get; set; }
        public bool Enabled { get; set; }
        public string Message { get; set; }
        public ulong ChannelId { get; set; }
        public string MinutesExpression { get; set; }
        public string HoursExpression { get; set; }
        public string DayOfMonthExpression { get; set; }
        public string MonthExpression { get; set; }
        public string DayOfWeekExpression { get; set; }

        public string GetCronExpression()
        {
            return $"{MinutesExpression} {HoursExpression} {DayOfMonthExpression} {MonthExpression} {DayOfWeekExpression}";
        }

        public string GetJobId()
        {
            return $"RRM:{Id.ToString()}";
        }
    }
}
